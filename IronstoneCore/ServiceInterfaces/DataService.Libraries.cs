using System;
using System.Collections.Generic;
using Jpp.Ironstone.Core.Properties;
using Jpp.Ironstone.Core.ServiceInterfaces.Library;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Jpp.Ironstone.Core.ServiceInterfaces
{
    public partial class DataService : IDataService
    {
        /// <summary>
        /// Load standard detail libraries
        /// </summary>
        private void LoadLibraries()
        {
            _logger.LogDebug("Beginning library load...");
            try
            {
                if (!GetRootLibraries()) return;

                _logger.LogDebug(String.Format(Resources.DataService_Inform_LoadingStandardLibraries, RootLibraries.Count));
                foreach (LibraryNode rootLibrary in RootLibraries)
                {
                    if (!rootLibrary.PreloadDisabled)
                    {
                        rootLibrary.Load();
                    }
                    else
                    {
                        _logger.LogDebug(String.Format(Resources.DataService_Inform_SkippingLibrary, rootLibrary.Name));
                    }
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Unexpected failure loading template libraries.");
            }
        }

        /// <summary>
        /// Get root libraries from settings
        /// </summary>
        /// <returns>Returns false if no libraries have been found</returns>
        private bool GetRootLibraries()
        {
            List<LibraryNode> nodes = new List<LibraryNode>();
            var sections = _settings.GetSection("standarddetaillibrary").GetChildren();

            foreach (IConfigurationSection configurationSection in sections)
            {
                LibraryNode newNode = new DirectoryNode();
                configurationSection.Bind(newNode, c => c.BindNonPublicProperties = true);
                nodes.Add(newNode);
                _logger.LogTrace($"Root library {newNode.Name} at path {newNode.Path} added.");
            }

            RootLibraries = nodes;

            _logger.LogDebug($"{RootLibraries.Count} root libraries found.");

            return RootLibraries != null;
        }
    }
}
