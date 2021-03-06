using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace FractalMachineLib.Develop
{
    public class ToAchieve
    {
        string achieveDir;

        public ToAchieve()
        {
            var assetsDir = Resources.Solve("Assets");
            achieveDir = assetsDir + "/ToAchieve/";
        }

        public void Achieve()
        {
            var dirInfo = new DirectoryInfo(achieveDir);
            var dirs = dirInfo.GetDirectories().OrderBy(f => f.Name).ToArray();
            var files = dirInfo.GetFiles().OrderBy(f => f.Name).ToArray();

            foreach (var dir in dirs)
            {
                var proj = new Project(dir.FullName);
                if (!File.Exists(proj.exeOutPath))
                {
                    proj.Compile();
                    System.Environment.Exit(0);
                }
            }

            foreach (var file in files)
            {
                if (file.Extension == ".light")
                {
                    var proj = new Project(file.FullName);
                    if (!File.Exists(proj.exeOutPath))
                    {
                        proj.Compile();
                        System.Environment.Exit(0);
                    }

                }
            }
        }
    }
}
