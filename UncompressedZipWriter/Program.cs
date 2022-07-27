Console.WriteLine("Let's Zip This!");

Zip64.ZipToFiles(@"C:\Users\spoda\Downloads\Test.zip", new[]
{
    @"C:\Users\spoda\Downloads\Datei 1.json",
    @"C:\Users\spoda\Downloads\Datei 2.json"
});

//Zip64.ZipToFiles(@"C:\Users\spoda\Downloads\Test.zip", new[]
//{
//    @"C:\Users\spoda\Downloads\Datei 1.mp4",
//    @"C:\Users\spoda\Downloads\Datei 2.mp4"
//});

//Zip64.ZipToFiles(@"C:\Users\spoda\Downloads\Test.zip", new[]
//{
//    @"C:\Users\spoda\Downloads\Datei 1.json",
//    @"C:\Users\spoda\Downloads\MEMORY.DMP",
//    @"C:\Users\spoda\Downloads\Datei 2.mp4"
//});


//Zip64.ZipToFiles(@"C:\Users\davep\Downloads\80days.zip", Directory.GetFiles(@"E:\Public\TV Shows\Around the World in 80 Days\Season 1"));