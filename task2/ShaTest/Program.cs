using System.Security.Cryptography;
using System.Text;
using System.Numerics;
using System.IO;
using System.Linq;


class Program
{
    static void Main(string[] args)
    {
        string folder = args.Length > 0 ? args[0] : "../data";
        var files = Directory.GetFiles(folder).Where(f => f.EndsWith(".data")).ToArray();

        if (files.Length != 256)
        {
            Console.WriteLine($"Expected 256 files, found {files.Length}");
        }

        var hashes = files.Select(file =>
        {
            byte[] data = File.ReadAllBytes(file);
            byte[] hashBytes = SHA3_256.HashData(data);
            string hash = Convert.ToHexString(hashBytes).ToLower();
            return new
            {
                File = file,
                Hash = hash,
                SortKey = ComputeSortKey(hash)
            };
        }).ToList();

        var sorted = hashes.OrderBy(x=>x.SortKey).Select(x=>x.Hash).ToList();

        string conc = string.Concat(sorted);

        string email = "khv.uzb14@gmail.com".ToLower();
        string finalstring = conc + email;

        byte[] finalHashBytes = SHA3_256.HashData(Encoding.UTF8.GetBytes(finalstring));
        string finalHash = Convert.ToHexString(finalHashBytes).ToLower();
        Console.WriteLine(finalHash);
    }

    static BigInteger ComputeSortKey(string hash)
    {
        BigInteger result = 1;
        foreach (char c in hash)
        {
            int value = Convert.ToInt32(c.ToString(), 16)+1;
            result *= value;
        }
        
        return result;
    }
       
}



// hashcode: 3e3d8bac95b25e58746abd2e60cab15b3cff261ce3b693ed7697d465011269a3 -- same as .js version result
