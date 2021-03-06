﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.AspNet.Http;

using Scissorhands.Helpers;
using Scissorhands.Models.Posts;
using Scissorhands.Models.Settings;
using Scissorhands.Services.Exceptions;
using Scissorhands.ViewModels.Post;

namespace Scissorhands.Services
{
    /// <summary>
    /// This represents the service entity for publish.
    /// </summary>
    public class PublishService : IPublishService
    {
        private const string PostPublishHtml = "/admin/post/publish/html";

        private readonly WebAppSettings _settings;
        private readonly ISiteMetadataSettings _metadata;
        private readonly IFileHelper _fileHelper;
        private readonly IHttpRequestHelper _requestHelper;

        private bool _disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="PublishService"/> class.
        /// </summary>
        /// <param name="settings"><see cref="WebAppSettings"/> instance.</param>
        /// <param name="metadata"><see cref="ISiteMetadataSettings"/> instance.</param>
        /// <param name="fileHelper"><see cref="IFileHelper"/> instance.</param>
        /// <param name="requestHelper"><see cref="IHttpRequestHelper"/> instance.</param>
        public PublishService(WebAppSettings settings, ISiteMetadataSettings metadata, IFileHelper fileHelper, IHttpRequestHelper requestHelper)
        {
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            this._settings = settings;

            if (metadata == null)
            {
                throw new ArgumentNullException(nameof(metadata));
            }

            this._metadata = metadata;

            if (fileHelper == null)
            {
                throw new ArgumentNullException(nameof(fileHelper));
            }

            this._fileHelper = fileHelper;

            if (requestHelper == null)
            {
                throw new ArgumentNullException(nameof(requestHelper));
            }

            this._requestHelper = requestHelper;
        }

        /// <summary>
        /// Applies metadata to the markdown body.
        /// </summary>
        /// <param name="model"><see cref="PostFormViewModel"/> instance.</param>
        /// <param name="metadata"><see cref="PublishedMetadata"/> instance.</param>
        /// <returns>Returns the markdown body with metadata applied.</returns>
        public string ApplyMetadata(PostFormViewModel model, PublishedMetadata metadata)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            if (metadata == null)
            {
                throw new ArgumentNullException(nameof(metadata));
            }

            var sb = new StringBuilder();
            sb.AppendLine("---");
            sb.AppendLine($"* Title: {metadata.Title}");
            sb.AppendLine($"* Slug: {metadata.Slug}");
            sb.AppendLine($"* Author: {metadata.Author}");
            sb.AppendLine($"* Date Published: {metadata.DatePublished.ToString(this._metadata.DateTimeFormat)}");
            sb.AppendLine($"* Tags: {string.Join(", ", metadata.Tags)}");
            sb.AppendLine("---");
            sb.AppendLine();
            sb.AppendLine(model.Body);

            return sb.ToString();
        }

        /// <summary>
        /// Publishes the markdown as a file.
        /// </summary>
        /// <param name="markdown">Content in Markdown format.</param>
        /// <param name="metadata"><see cref="PublishedMetadata"/> instance.</param>
        /// <returns>Returns the Markdown file path in a virtual path format.</returns>
        public async Task<string> PublishMarkdownAsync(string markdown, PublishedMetadata metadata)
        {
            if (string.IsNullOrWhiteSpace(markdown))
            {
                throw new ArgumentNullException(nameof(markdown));
            }

            if (metadata == null)
            {
                throw new ArgumentNullException(nameof(metadata));
            }

            var markdownpath = $"{this._settings.MarkdownPath}/{metadata.DatePublished.ToString("yyyy/MM/dd")}";
            var filename = $"{metadata.Slug}.md";
            var filepath = Path.Combine(this._fileHelper.GetDirectory(markdownpath), filename);

            var written = await this._fileHelper.WriteAsync(filepath, markdown).ConfigureAwait(false);
            if (!written)
            {
                throw new PublishFailedException("Markdown not published");
            }

            return $"{markdownpath}/{filename}";
        }

        /// <summary>
        /// Publishes the HTML post as a file.
        /// </summary>
        /// <param name="html">Content in HTML format.</param>
        /// <param name="metadata"><see cref="PublishedMetadata"/> instance.</param>
        /// <returns>Returns the HTML file path.</returns>
        public async Task<string> PublishHtmlAsync(string html, PublishedMetadata metadata)
        {
            if (string.IsNullOrWhiteSpace(html))
            {
                throw new ArgumentNullException(nameof(html));
            }

            if (metadata == null)
            {
                throw new ArgumentNullException(nameof(metadata));
            }

            var htmlpath = $"{this._settings.HtmlPath}/{metadata.DatePublished.ToString("yyyy/MM/dd")}";
            var filename = $"{metadata.Slug}.html";
            var filepath = Path.Combine(this._fileHelper.GetDirectory(htmlpath), filename);

            var written = await this._fileHelper.WriteAsync(filepath, html).ConfigureAwait(false);
            if (!written)
            {
                throw new PublishFailedException("Post not published");
            }

            return $"{htmlpath}/{filename}";
        }

        /// <summary>
        /// Gets the published HTML content.
        /// </summary>
        /// <param name="model"><see cref="PostFormViewModel"/> instance.</param>
        /// <param name="request"><see cref="HttpRequest"/> instance.</param>
        /// <returns>Returns the published HTML content.</returns>
        public async Task<string> GetPublishedHtmlAsync(PostFormViewModel model, HttpRequest request)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            using (var client = this._requestHelper.CreateHttpClient(request, PublishMode.Parse))
            using (var content = this._requestHelper.CreateStringContent(model))
            {
                var response = await client.PostAsync(PostPublishHtml, content).ConfigureAwait(false);
                var html = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                return html;
            }
        }

        /// <summary>
        /// Publishes the post as a file.
        /// </summary>
        /// <param name="model"><see cref="PostFormViewModel"/> instance.</param>
        /// <param name="request"><see cref="HttpRequest"/> instance.</param>
        /// <returns>Returns the <see cref="PublishedPostPath"/> instance containing paths for published files.</returns>
        public async Task<PublishedPostPath> PublishPostAsync(PostFormViewModel model, HttpRequest request)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            var publishedpath = new PublishedPostPath();

            var metadata = this.GetMetadata(model);

            var markdown = this.ApplyMetadata(model, metadata);

            var markdownpath = await this.PublishMarkdownAsync(markdown, metadata).ConfigureAwait(false);
            publishedpath.Markdown = markdownpath;

            var html = await this.GetPublishedHtmlAsync(model, request).ConfigureAwait(false);

            var htmlpath = await this.PublishHtmlAsync(html, metadata).ConfigureAwait(false);
            publishedpath.Html = htmlpath;

            return publishedpath;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            if (this._disposed)
            {
                return;
            }

            this._disposed = true;
        }

        private PublishedMetadata GetMetadata(PostFormViewModel model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            var metadata = new PublishedMetadata()
                               {
                                   Title = model.Title,
                                   Slug = model.Slug,
                                   Author = model.Author,
                                   DatePublished = model.DatePublished,
                                   Tags = GetTags(model.Tags),
                               };

            return metadata;
        }

        private static List<string> GetTags(string tags)
        {
            if (string.IsNullOrWhiteSpace(tags))
            {
                return null;
            }

            var list = tags.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries).Select(p => p.Trim()).ToList();
            return list;
        }
    }
}