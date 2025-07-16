using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Core.Models;
using ConfigModel = Core.Models.Config;

namespace Services.Config
{
    public class ConfigService
    {
        private readonly string _configPath;
        private ConfigModel _config;

        public ConfigService(string configPath = "config.json")
        {
            _configPath = configPath;
            _config = new ConfigModel();
        }

        public ConfigModel Load()
        {
            if (!File.Exists(_configPath))
            {
                throw new FileNotFoundException($"Config file not found: {_configPath}");
            }

            var json = File.ReadAllText(_configPath);
            var config = JsonSerializer.Deserialize<ConfigModel>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (config == null)
            {
                throw new InvalidOperationException("Failed to deserialize config");
            }

            // Expand routes to generate flattened mapping
            ExpandRoutes(config);
            
            _config = config;
            return config;
        }

        public void Save(ConfigModel config)
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };
            var json = JsonSerializer.Serialize(config, options);
            File.WriteAllText(_configPath, json);
        }

        private void ExpandRoutes(ConfigModel config)
        {
            const int bytesPerLED = 3;
            const int evenCapBytes = 170 * bytesPerLED; // 510
            const int oddCapBytes = 89 * bytesPerLED;   // 255

            var outMapping = new List<MapEntry>();

            foreach (var route in config.Routes)
            {
                if (!config.Groups.ContainsKey(route.Group) || !config.Universes.ContainsKey(route.UniBank))
                {
                    continue; // Skip invalid routes
                }

                var ranges = config.Groups[route.Group];
                var destSet = config.Universes[route.UniBank];

                if (ranges.Count == 0 || destSet.Count == 0)
                {
                    continue;
                }

                int uniIdx = 0;
                int chanPos = route.Channel - 1; // zero-based within current universe

                int GetCapBytes(ushort universe)
                {
                    return universe % 2 == 0 ? evenCapBytes : oddCapBytes;
                }

                foreach (var range in ranges)
                {
                    var (from, to) = ParseRange(range);
                    
                    for (uint ent = from; ent <= to; ent++)
                    {
                        // Ensure we have a universe and space for 1 LED (=3 bytes)
                        while (uniIdx < destSet.Count && 
                               chanPos + bytesPerLED > GetCapBytes(destSet[uniIdx].Universe))
                        {
                            uniIdx++;
                            chanPos = route.Channel - 1; // restart offset for next universe
                        }

                        if (uniIdx >= destSet.Count)
                        {
                            break; // no more destinations available
                        }

                        var dst = destSet[uniIdx];
                        outMapping.Add(new MapEntry
                        {
                            Entity = ent,
                            Controller = dst.IP,
                            Universe = (byte)dst.Universe,
                            Channel = (ushort)(chanPos + 1), // convert back to 1-based
                            SelectRGBW = route.Select,
                            Enable = route.Enable
                        });

                        chanPos += bytesPerLED;
                    }
                }
            }

            config.Mapping = outMapping;
        }

        private (uint from, uint to) ParseRange(string range)
        {
            var dashIndex = range.IndexOf('-');
            if (dashIndex > 0)
            {
                var fromStr = range.Substring(0, dashIndex);
                var toStr = range.Substring(dashIndex + 1);
                
                if (uint.TryParse(fromStr, out var from) && uint.TryParse(toStr, out var to))
                {
                    return (from, to);
                }
            }

            // Single value
            if (uint.TryParse(range, out var value))
            {
                return (value, value);
            }

            return (0, 0);
        }

        public ConfigModel GetCurrentConfig() => _config;
    }
} 