using System;
using System.Collections.Generic;
using System.Windows.Media.Imaging;

namespace ShowcaseApp.WPF.Models
{
    public static class ThemedDataStorage
    {
        public static readonly List<string> Gender = new List<string>()
        {
            @"Female",
            @"Male",
        };

        public static readonly List<string> Professions = new List<string>()
        {
            "Expeditor",
            "Support",
            "Analyst",
            "Lead Analyst",
            "Developer",
            "Lead developer",
            "Manager",
            "Secretary",
            "Engineer",
            "Software engineer",
            "Architect",
            "Lead Architect",
            "Project manager",
            "Project Lead",
            "Finance Manager",
            "Executive Director"
        };

        public static readonly List<string> Names = new List<string>()
        {
            "Madeline Haggerty",
            "Eldon Deaver",
            "Latoria Flynn",
            "Adela Tally",
            "Rory Fuhrman",
            "Melony Lovato",
            "Nelia Maples",
            "Londa Fagin",
            "Nicki Mcleod",
            "Dante Bialaszewski",
            "Reba Vidrine",
            "Ruthie Boatman",
            "Linette Tarpey",
            "Georgianne Colangelo",
            "Jeff Roseman",
            "Halley Bormann",
            "Johnsie Vanburen",
            "Maximo Okada",
            "Marion Alverson",
            "Lottie Rhinehart",
            "Maddie Beckmann",
            "Cordell Carper",
            "Joanne Gibbon",
            "Tianna Gilfillan",
            "Hyman Guttierrez",
            "Cheri Duclos",
            "Adrienne Labrum",
            "Ciera Hurston",
            "Tamra Bohling",
            "Major Bray",
            "Debbra Leith",
            "Claudine Swensen",
            "Modesto Cebula",
            "Maryann Hosch",
            "Annemarie Brutus",
            "Lena Bowerman",
            "Parker Rentfro",
            "Nathaniel Markel",
            "Huey Mefford",
            "Justina Cushenberry",
            "Lashaun Hunsicker",
            "Raymonde Mcfate",
            "Kiesha Jenny",
            "Peggy Vroman",
            "Reinaldo Vella",
            "Margot Starck",
            "Octavia Maddem",
            "Dreama Fairfield",
            "Bobby Donnelly",
            "Pattie Schaffner",
            "Carylon Cheever",
            "Chana Stuber",
            "Bong Myatt",
            "Porfirio Garney",
            "Leann Toki",
            "Becki Feinberg",
            "Jonna Stallone",
            "Ilene Blizzard",
            "Yasuko Prigge",
            "Diego Farone",
            "Anh Gerace",
            "Isa Stukes",
            "Evelina Patz",
            "Shanell Raschke",
            "Bella Borda",
            "Laureen Jarmon",
            "Mariam Keever",
            "Olga Corle",
            "Otis Turner",
            "Katrice Hisle",
            "Corey Newnam",
            "Adan Dosch",
            "Sonny Patricio",
            "Sanda Arner",
            "Rolando Fitzgerald",
            "Annie Gatchell",
            "Suanne Bosque",
            "Anibal Terrio",
            "Alethea Guay",
            "Wilburn Takacs",
            "Forrest Aburto",
            "Zana Sikorski",
            "Wendi Gosselin",
            "Savannah Duggins",
            "Carrol Kerr",
            "Freeda Pharis",
            "Beaulah Santini",
            "Coy Dowd",
            "Kareem Arterburn",
            "Annamae Fortner",
            "Maxwell Bordelon",
            "Julietta Barajas",
            "Darcel Schur",
            "Vena Auten",
            "Audrea Roher",
            "Emory Olaughlin",
            "Deandrea Purinton",
            "Kacy Macek",
            "Lona Engler",
            "Oren Kesten",
        };

        public static readonly List<BitmapImage> Images = new List<BitmapImage>();
        public static readonly List<BitmapImage> EditorImages = new List<BitmapImage>(); 

        static ThemedDataStorage()
        {
            Images.Add(new BitmapImage(new Uri(@"pack://application:,,,/ShowcaseApp.WPF;component/Assets/female.png", UriKind.Absolute)) { CacheOption = BitmapCacheOption.OnLoad });
            Images.Add(new BitmapImage(new Uri(@"pack://application:,,,/ShowcaseApp.WPF;component/Assets/male.png", UriKind.Absolute)) { CacheOption = BitmapCacheOption.OnLoad });
            EditorImages.Add(new BitmapImage(new Uri(@"pack://application:,,,/ShowcaseApp.WPF;component/Assets/hand_comp.png", UriKind.Absolute)) { CacheOption = BitmapCacheOption.OnLoad });
            EditorImages.Add(new BitmapImage(new Uri(@"pack://application:,,,/ShowcaseApp.WPF;component/Assets/hand_comp2.png", UriKind.Absolute)) { CacheOption = BitmapCacheOption.OnLoad });
            EditorImages.Add(new BitmapImage(new Uri(@"pack://application:,,,/ShowcaseApp.WPF;component/Assets/hand_comp3.png", UriKind.Absolute)) { CacheOption = BitmapCacheOption.OnLoad });
        }

        public static BitmapImage GetImageById(int imageId)
        {
            return Images[imageId >= 1 ? 1 : 0];
        }

        public static BitmapImage GetEditorImageById(int imageId)
        {
            return EditorImages[imageId >= 1 ? 1 : 0];
        }

        public static void FillDataVertex(DataVertex item)
        {
            item.Age = ShowcaseHelper.Rand.Next(18, 75);
            item.Gender = Gender[ShowcaseHelper.Rand.Next(0, 1)];
           // item.ImageId = item.Gender == ThemedDataStorage.Gender[0] ? 0 : 1;
            item.Profession = Professions[ShowcaseHelper.Rand.Next(0, Professions.Count - 1)];
            item.Name = Names[ShowcaseHelper.Rand.Next(0, Names.Count - 1)];
        }
    }
}
