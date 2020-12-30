
using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Configuration;

namespace full_text_search.configuration {
    
    public class Configuration {

        private readonly IConfiguration configuration;

        public uint InvertedIndexCacheSize { get; private set; }
        public IList<string> AllowedExtensions { get; private set; }
        
        public Configuration() {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json");

            this.configuration = builder.Build();
            
            this.populateConfigValues();
        }

        private void populateConfigValues() {
            this.InvertedIndexCacheSize = Convert.ToUInt32(Int32.Parse(this.configuration["InvertedIndexCacheSize"]));
            this.AllowedExtensions = new List<string>();
            this.configuration.GetSection("AllowedExtensions").Bind(this.AllowedExtensions);
            
            this.validateConfigValues();
        }

        private void validateConfigValues() {
            if (InvertedIndexCacheSize < 1 || this.InvertedIndexCacheSize > 1000) {
                throw new ArgumentException("Configuration:InvertedIndexCacheSize must be between 1 and 1000");
            }

            if (this.AllowedExtensions == null || this.AllowedExtensions.Count == 0) {
                throw new ArgumentException("Configuration:AllowedExtensions must have at least 1 valid extension");
            }
        }
    }
} 