﻿using System;

using Aliencube.Scissorhands.Models;

using Moq;

namespace Aliencube.Scissorhands.Services.Tests.Fixtures
{
    /// <summary>
    /// This represents the fixture entity for the <see cref="ThemeService"/> class.
    /// </summary>
    public class ThemeServiceFixture : IDisposable
    {
        private bool _disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="ThemeServiceFixture"/> class.
        /// </summary>
        public ThemeServiceFixture()
        {
            this.WebAppSettings = new Mock<WebAppSettings>();
            this.ThemeService = new ThemeService(this.WebAppSettings.Object);
        }

        /// <summary>
        /// Gets the <see cref="Mock{WebAppSettings}"/> instance.
        /// </summary>
        public Mock<WebAppSettings> WebAppSettings { get; }

        /// <summary>
        /// Gets the <see cref="ThemeService"/> instance.
        /// </summary>
        public IThemeService ThemeService { get; }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            if (this._disposed)
            {
                return;
            }

            this.ThemeService.Dispose();

            this._disposed = true;
        }
    }
}