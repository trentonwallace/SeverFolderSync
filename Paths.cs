//-----------------------------------------------------------------------------------
//  Description : Custom getfiles class
//  Author Name : Trent Wallace
//  Date Written: 06/05/2010
//  Revision Log: 
//  added logging to DB
//  returns all the files when given a path
//-----------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace WebFolderSync
{
    static class Paths
    {
        
    public static string[] GetFiles(string folder)
    {
        List<string> validFilesPaths = new List<string>();

        if (!Directory.Exists(folder))
            return validFilesPaths.ToArray();

        string[] filePaths = Directory.GetFiles(folder, "*.*", SearchOption.AllDirectories);
        
        foreach (string filePath in filePaths)
        {
                validFilesPaths.Add(filePath.Substring(folder.Length));
        }

        return validFilesPaths.ToArray();
    }

    public static string[] GetFiles(string folder,string excludeFolder)
    {
        char seperator = Path.DirectorySeparatorChar;
        List<string> validFilesPaths = new List<string>();

        if (!Directory.Exists(folder))
            return validFilesPaths.ToArray();

        string[] filePaths = Directory.GetFiles(folder, "*.*", SearchOption.AllDirectories);
        
        foreach (string filePath in filePaths)
        {
            //removes exclude folder
            if (!filePath.Contains(seperator + excludeFolder + seperator))
                validFilesPaths.Add(filePath.Substring(folder.Length));
        }

        return validFilesPaths.ToArray();
    }

    public static List<PathEntity> GetPathEntities(string folder)
    {
        char seperator = Path.DirectorySeparatorChar;
        List<PathEntity> PathEntities = new List<PathEntity>();

        if (!Directory.Exists(folder))
            return PathEntities;
        
        string[] filePaths = Directory.GetFiles(folder, "*.*", SearchOption.AllDirectories);
        
        foreach (string filePath in filePaths)
        {
            PathEntities.Add(new PathEntity(filePath, filePath.Substring(folder.Length), File.GetLastWriteTime(filePath)));
        }

        return PathEntities;
    }

    public static List<PathEntity> GetPathEntities(string folder, string excludeFolder)
    {
        List<PathEntity> PathEntities = new List<PathEntity>();
        char seperator = Path.DirectorySeparatorChar;
        try
        {

            if (!Directory.Exists(folder))
                return PathEntities;

            string[] filePaths = Directory.GetFiles(folder, "*.*", SearchOption.AllDirectories);

            foreach (string filePath in filePaths)
            {
                //removes exclude folder
                if (!filePath.Contains(excludeFolder + seperator))
                {
                    PathEntities.Add(new PathEntity(filePath, filePath.Substring(folder.Length), File.GetLastWriteTime(filePath)));
                }
            }
        }
        catch (Exception ex) { LogError.Log("Program:WebFolderSync;Class:Paths;Method:GetPathEntities(,)" + ex.Message); }

        return PathEntities;
    }

    public static List<PathEntity> GetPathEntities(string folder1,string folder2,string folder3, string excludeFolder)
    {
        List<PathEntity> PathEntities = new List<PathEntity>();
        char seperator = Path.DirectorySeparatorChar;

        try
        {
            if (!Directory.Exists(folder1))
                return PathEntities;

            if (!Directory.Exists(folder2))
                return PathEntities;

            if (!Directory.Exists(folder3))
                return PathEntities;

            string[] filePaths1 = Directory.GetFiles(folder1, "*.*", SearchOption.AllDirectories);
            string[] filePaths2 = Directory.GetFiles(folder2, "*.*", SearchOption.AllDirectories);
            string[] filePaths3 = Directory.GetFiles(folder3, "*.*", SearchOption.AllDirectories);

            foreach (string filePath1 in filePaths1)
            {
                //removes exclude folder
                if (!filePath1.Contains(excludeFolder + seperator))
                {
                    PathEntities.Add(new PathEntity(filePath1, filePath1.Substring(folder1.Length), File.GetLastWriteTime(filePath1)));
                }
            }

            foreach (string filePath2 in filePaths2)
            {
                //removes exclude folder
                if (!filePath2.Contains(excludeFolder + seperator))
                {
                    PathEntities.Add(new PathEntity(filePath2, filePath2.Substring(folder2.Length), File.GetLastWriteTime(filePath2)));
                }
            }

            foreach (string filePath3 in filePaths3)
            {
                //removes exclude folder
                if (!filePath3.Contains(excludeFolder + seperator))
                {
                    PathEntities.Add(new PathEntity(filePath3, filePath3.Substring(folder3.Length), File.GetLastWriteTime(filePath3)));
                }
            }
        }
        catch (Exception ex) { LogError.Log("Program:WebFolderSync;Class:Paths;Method:GetPathEntities(,,,)" + ex.Message); }

        return PathEntities;
    }
    }
}
