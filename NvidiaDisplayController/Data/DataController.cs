using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Newtonsoft.Json;
using NvidiaDisplayController.Objects;

namespace NvidiaDisplayController.Data;

public class DataController
{
    private static readonly string _location = Assembly.GetExecutingAssembly().Location;
    private static readonly string? _directory = Path.GetDirectoryName(_location);

    public string DataPath
    {
        get
        {
            if (_directory != null)
                return Path.Combine(_directory, @"Data\Data.json");
            throw new Exception();
        }
    }

    public void Write(List<Monitor> monitors)
    {
        var serializeObject = JsonConvert.SerializeObject(monitors, new JsonSerializerSettings
        {
            PreserveReferencesHandling = PreserveReferencesHandling.Objects
        });
        File.WriteAllText(DataPath, serializeObject);
    }

    public List<Monitor>? Load()
    {
        using StreamReader reader = new(DataPath);
        {
            var json = reader.ReadToEnd();
            var monitors = JsonConvert.DeserializeObject<List<Monitor>>(json);
            reader.Close();
            
            return monitors;
        }
    }
}