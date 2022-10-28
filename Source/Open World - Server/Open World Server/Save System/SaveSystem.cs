﻿using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using OpenWorldServer.Services;

namespace OpenWorldServer
{
    public class SaveSystem
    {
        public static void SaveBannedIPs(Dictionary<string, string> IPs)
        {
            string folderPath = PathProvider.MainFolderPath;
            string filepath = folderPath + Path.DirectorySeparatorChar + "Banned IPs.data";

            BinaryFormatter formatter = new BinaryFormatter();

            FileStream stream = new FileStream(filepath, FileMode.Create);

            BanDataHolder data = new BanDataHolder(IPs);

            formatter.Serialize(stream, data);

            stream.Close();
        }

        public static BanDataHolder LoadBannedIPs()
        {
            string path = PathProvider.MainFolderPath + Path.DirectorySeparatorChar + "Banned IPs.data";

            BinaryFormatter formatter = new BinaryFormatter();

            FileStream stream = new FileStream(path, FileMode.Open);

            BanDataHolder data = formatter.Deserialize(stream) as BanDataHolder;

            stream.Close();

            return data;
        }
    }
}
