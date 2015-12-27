﻿using System;
using System.Threading.Tasks;

using Aliencube.Scissorhands.ViewModels.Post;

using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Mvc.ViewFeatures;

namespace Aliencube.Scissorhands.Services
{
    /// <summary>
    /// This provides interfaces to the <see cref="PublishService"/> class.
    /// </summary>
    public interface IPublishService : IDisposable
    {
        /// <summary>
        /// Gets the HTML to be published.
        /// </summary>
        /// <param name="resolver"><see cref="IServiceProvider"/> instance.</param>
        /// <param name="actionContext"><see cref="ActionContext"/> instance.</param>
        /// <param name="viewModel"><see cref="PostPublishViewModel"/> instance.</param>
        /// <param name="viewData"><see cref="ViewDataDictionary"/> instance.</param>
        /// <param name="tempData"><see cref="ITempDataDictionary"/> instance.</param>
        /// <returns>Returns HTML to be published.</returns>
        Task<string> GetPublishHtmlAsync(IServiceProvider resolver, ActionContext actionContext, PostPublishViewModel viewModel, ViewDataDictionary viewData, ITempDataDictionary tempData);
    }
}