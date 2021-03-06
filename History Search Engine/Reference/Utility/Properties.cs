﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Reference.Utility
{
    public class Properties : Dictionary<String, String>
    {
        public void Load(String path)
        {
            StreamReader reader = new StreamReader(File.OpenRead(path));
            String line = null;

            Clear();

            while ((line = reader.ReadLine()) != null)
            {
                line = line.Trim();

                if (line.Length == 0)
                {
                    continue;
                }

                if (line.StartsWith("#"))
                {
                    continue;
                }

                switch (line.Count(separator => separator == '='))
                {
                    case 1:
                        String[] pair = line.Split('=');

                        if (pair.Length == 1)
                        {
                            continue;
                        }

                        Add(pair[0], pair[1]);
                        break;
                    default:
                        throw new Exception("프로퍼티 설정 파일이 잘못되었습니다.");
                }
            }
        }
    }
}
