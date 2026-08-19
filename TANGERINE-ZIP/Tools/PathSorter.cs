using System;
using System.Linq;
using System.Runtime.InteropServices;

public static class PathSorter
{
    // Windows shell logical string comparison - exactly matches Explorer's "Name" column sort:
    // - Chinese characters sorted by pinyin (when OS locale is zh-CN)
    // - Numbers sorted as numbers (file2 before file10)
    // - Other languages follow the system locale sort order
    [DllImport("shlwapi.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
    private static extern int StrCmpLogicalW(string psz1, string psz2);

    // Merge two string arrays and sort them the way Explorer sorts file names
    public static string[] MergeAndSort(string[] first, string[] second)
    {
        // Concat merges the two arrays, then sort with the Explorer-matching comparer
        return first.Concat(second)
                    .OrderBy(p => p, Comparer<string>.Create(StrCmpLogicalW))
                    .ToArray();
    }
}
//usage:
//for example
/*
 string[] files = Directory.GetFiles(folderPath);
string[] dirs  = Directory.GetDirectories(folderPath);

string[] sorted = PathSorter.MergeAndSort(files, dirs);
// sorted now contains files + folders, sorted exactly like Explorer's Name column
 */
//written by workbuddy on 20260816 at first