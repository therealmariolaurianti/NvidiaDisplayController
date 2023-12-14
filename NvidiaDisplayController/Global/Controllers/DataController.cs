using System;
using System.IO;
using System.Reflection;
using FluentResults;
using Newtonsoft.Json;
using NvidiaDisplayController.Objects;

namespace NvidiaDisplayController.Global.Controllers;

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

    public void Write(Computer data)
    {
        var serializeObject = JsonConvert.SerializeObject(data, new JsonSerializerSettings
        {
            PreserveReferencesHandling = PreserveReferencesHandling.Objects
        });

        File.WriteAllText(DataPath, serializeObject);
    }

    public Result<Computer> Load()
    {
        using StreamReader reader = new(DataPath);
        {
            var json = reader.ReadToEnd();

            var computer = JsonConvert.DeserializeObject<Computer>(json);
            reader.Close();

            var result = computer is null ? Result.Fail(new Error("")) : Result.Ok(computer);
            return result;
        }
    }
}