Console.WriteLine("Let's Zip This!");

Zip64.ZipToFiles(@"C:\Users\davep\Downloads\Test.zip", new[]
{
    @"C:\Users\davep\Downloads\Datei 1.txt",
    @"C:\Users\davep\Downloads\Datei 2.txt"
});


//Zip64.ZipToFiles(@"C:\Users\davep\Downloads\80days.zip", Directory.GetFiles(@"E:\Public\TV Shows\Around the World in 80 Days\Season 1"));