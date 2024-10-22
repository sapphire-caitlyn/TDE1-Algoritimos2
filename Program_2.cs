//using System;
//using System.Diagnostics;
//using System.Reflection.Metadata;
//using System.Text;

//namespace TDE_1 {
//    static class Program {

//    static void Main() {
//        //Clear Extension Area
//        FileInfo FI = new FileInfo(Path.Combine(Directory.GetCurrentDirectory(), EXTENSION_BINARY_FILENAME));
//        if (FI.Exists) {
//            FI.Delete();
//        }

//        CreateFiles();

//        AddMovie(new Movie(6644200, "A Quiet Place", 2018, "https://www.imdb.com/title/tt6644200/", 0));

//        FileStream FsBinaryFile = new(Path.Combine(Directory.GetCurrentDirectory(), BINARY_FILENAME), FileMode.Open, FileAccess.Read);
//        FileStream FsExtensionFile = new(Path.Combine(Directory.GetCurrentDirectory(), EXTENSION_BINARY_FILENAME), FileMode.OpenOrCreate, FileAccess.Read);
//        FileStream FsIdIndexFile = new(Path.Combine(Directory.GetCurrentDirectory(), ID_INDEX_FILENAME), FileMode.Open, FileAccess.Read);
//        long idMovie = 6644200;
//        BinarySearchIndexIdFile(FsIdIndexFile, FsBinaryFile, FsExtensionFile, idMovie);
//    }

//    static void AddMovie(Movie Movie) {
//        FileStream Fs = new(Path.Combine(Directory.GetCurrentDirectory(), EXTENSION_BINARY_FILENAME), FileMode.OpenOrCreate, FileAccess.ReadWrite);
//        Fs.Position = Fs.Length;
//        Fs.Write(Movie);

//        FileStream FsIndex = new(Path.Combine(Directory.GetCurrentDirectory(), ID_INDEX_FILENAME), FileMode.OpenOrCreate, FileAccess.ReadWrite);
//        List<IndexObject<long>> lst = new();
//        for(int i = 0; i < ((int)FsIndex.Length / ID_INDEX_SIZE); i++) {
//            lst.Add(ReadIndex<long>(FsIndex.ReadBytes(ID_INDEX_SIZE), sizeof(long)));
//        }
//        lst.Add(new IndexObject<long>() { Content = Movie.idMovie, Index = ((int)Fs.Position / CLASS_SIZE) - 1, ContentSize = sizeof(long), ExtensionArea = true});
//        lst.OrderBy(p => p.Content);
//        FsIndex.Position = 0;
//        foreach (var item in lst) {
//            FsIndex.Write(item);
//        }

//        FsIndex.Close();
//        Fs.Close();
//    }

//    /// <summary>
//    /// Cria arquivo binario e arquivos de index 
//    /// </summary>
//    static void CreateFiles() {
//        string FullFileMovies = File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), ORIGINAL_FILE_NAME));
//        string[][] lstMoviesArr = FullFileMovies.Split('\n').Select(p => p.Split(';')).Skip(1).ToArray();
//        List<Movie> lstMovies = lstMoviesArr.Select(p => new Movie(p)).OrderBy(p => p.Title).Where(p => p.idMovie != 0).ToList();

//        CreateBinaryFile(lstMovies);
//        CreateTitleIndexFile(lstMovies);
//        CreateIdIndexFile(lstMovies);
//    }

//    static void BinarySearchIndexIdFile(FileStream FsIndex, FileStream FsBinary, FileStream FsExtension, long idMovie) {
//        Stopwatch Timer = new Stopwatch();
//        Timer.Start();
//        FsIndex.Position = 0;

//        //Load
//        Tuple<int, long, bool> tIndex = BinarySearchIdMovie(FsIndex, idMovie);
//        Movie? movie = null;
//        if (tIndex.Item3) {
//            movie = ReadMovieAt(FsExtension, tIndex.Item1);
//        } else {
//            movie = ReadMovieAt(FsBinary, tIndex.Item1);
//        }

//        Timer.Stop();
//        Console.WriteLine($"Binary search in file took {Timer.ElapsedMilliseconds}ms, found {movie.Title}");
//    }

//    static Tuple<int, long, bool> BinarySearchIdMovie(FileStream Fs, long idMovie) {
//        long low = 0, high = Fs.Length / ID_INDEX_SIZE, mid = (low + high) / 2;

//        while (low <= high) {
//            Fs.Position = mid;
//            Tuple<int, long, bool> t = ReadIndexAt<long>(Fs, mid, sizeof(long));
//            long id = t.Item2;

//            if (id == idMovie) { return t; } else
//            if (id > idMovie) { high = mid - 1; } else
//            if (id < idMovie) { low = mid + 1; }

//            mid = (low + high) / 2;
//        }
//        return new Tuple<int, long, bool>(-1, -1, false);
//    }

//    static Tuple<int, T, bool> ReadIndexAt<T>(FileStream Fs, long middlePosition, int ContentSize) {
//        int indexObjectSize = ContentSize + 4 + sizeof(bool);

//        byte[] buffer = new byte[indexObjectSize];

//        Fs.Seek(middlePosition * indexObjectSize, SeekOrigin.Begin);
//        Fs.Read(buffer, 0, buffer.Length);

//        byte[] ContentBuffer = buffer.SkipLast(4 + sizeof(bool)).ToArray();
//        byte[] indexBuffer = buffer.Skip(ContentSize).SkipLast(sizeof(bool)).ToArray();
//        byte[] isExtensionBuffer = buffer.Skip(indexObjectSize - sizeof(bool)).ToArray();

//        T? Res = default;
//        if (typeof(T) == typeof(string)) {
//            Res = (T)Convert.ChangeType(Encoding.Default.GetString(ContentBuffer).Trim(), typeof(T));
//        } else
//        if (typeof(T) == typeof(int)) {
//            Res = (T)Convert.ChangeType(BitConverter.ToInt32(ContentBuffer), typeof(T));
//        } else
//        if (typeof(T) == typeof(long)) {
//            Res = (T)Convert.ChangeType(BitConverter.ToInt64(ContentBuffer), typeof(T));
//        }

//        int index = BitConverter.ToInt32(indexBuffer);
//        bool isExtension = BitConverter.ToBoolean(isExtensionBuffer);

//        return new(index, Res!, isExtension);
//    }

//    static IndexObject<T> ReadIndex<T>(byte[] buffer, int ContentSize) {
//        int indexObjectSize = ContentSize + 4 + sizeof(bool);

//        byte[] ContentBuffer = buffer.SkipLast(4 + sizeof(bool)).ToArray();
//        byte[] indexBuffer = buffer.Skip(ContentSize).SkipLast(sizeof(bool)).ToArray();
//        byte[] isExtensionBuffer = buffer.Skip(indexObjectSize - sizeof(bool)).ToArray();

//        T? Res = default;
//        if (typeof(T) == typeof(string)) {
//            Res = (T)Convert.ChangeType(Encoding.Default.GetString(ContentBuffer).Trim(), typeof(T));
//        } else
//        if (typeof(T) == typeof(int)) {
//            Res = (T)Convert.ChangeType(BitConverter.ToInt32(ContentBuffer), typeof(T));
//        } else
//        if (typeof(T) == typeof(long)) {
//            Res = (T)Convert.ChangeType(BitConverter.ToInt64(ContentBuffer), typeof(T));
//        }

//        int index = BitConverter.ToInt32(indexBuffer);
//        bool isExtension = BitConverter.ToBoolean(isExtensionBuffer);

//        return new IndexObject<T>() {
//            Content = Res!,
//            ExtensionArea = isExtension,
//            Index = index,
//            ContentSize = ContentSize
//        };
//    }

//    static Movie ReadMovieAt(FileStream Fs, long middlePosition) {
//        var buffer = new byte[CLASS_SIZE];

//        Fs.Seek(middlePosition * CLASS_SIZE, SeekOrigin.Begin);
//        Fs.Read(buffer, 0, buffer.Length);

//        return new Movie(buffer);
//    }

//    static void CreateBinaryFile(List<Movie> lstMovies) {

//        FileStream Fs = new FileStream(Path.Combine(Directory.GetCurrentDirectory(), BINARY_FILENAME), FileMode.Create, FileAccess.Write);
//        foreach (Movie movie in lstMovies) {
//            Fs.Write(movie);
//        }
//        Fs.Close();
//    }

//    static void CreateIdIndexFile(List<Movie> lstMovies) {
//        FileStream Fs = new FileStream(Path.Combine(Directory.GetCurrentDirectory(), ID_INDEX_FILENAME), FileMode.Create, FileAccess.Write);
//        IEnumerable<Movie> lstMoviesIdOrdered = lstMovies.OrderBy(p => p.idMovie);
//        foreach (Movie movie in lstMoviesIdOrdered) {
//            long idMovie = movie.idMovie;
//            int index = lstMovies.IndexOf(movie);
//            IndexObject<long> indexObject = new() {
//                Content = idMovie,
//                ExtensionArea = false,
//                Index = index,
//                ContentSize = sizeof(long)
//            };
//            Fs.Write(indexObject);
//        }
//        Fs.Close();
//    }

//    static void CreateTitleIndexFile(List<Movie> lstMovies) {
//        FileStream Fs = new FileStream(Path.Combine(Directory.GetCurrentDirectory(), TITLE_INDEX_FILENAME), FileMode.Create, FileAccess.Write);

//        foreach (Movie movie in lstMovies) {
//            string Title = movie.Title;
//            int index = lstMovies.IndexOf(movie);

//            IndexObject<string> indexObject = new() {
//                Content = Title,
//                ExtensionArea = false,
//                Index = index,
//                ContentSize = STRING_SIZE
//            };

//            Fs.Write(indexObject);
//        }
//        Fs.Close();
//    }

//    static readonly int STRING_SIZE = 150;
//    static readonly int CLASS_SIZE = 8 + STRING_SIZE + 4 + STRING_SIZE + 8 + 4;


//    static readonly string ID_INDEX_FILENAME = "id_index.bin";
//    static readonly int ID_INDEX_SIZE = 8 + 4 + sizeof(bool);//[ IDMOVIE,INDEX,ISEXTENSION ]

//    static readonly string TITLE_INDEX_FILENAME = "title_index_PK_.bin";
//    static readonly int TITLE_INDEX_SIZE = STRING_SIZE + 4 + sizeof(bool);//[ TITLE,INDEX,ISEXTENSION ]

//    static readonly string BINARY_FILENAME = "movies.bin";
//    static readonly string EXTENSION_BINARY_FILENAME = "extension_movies.bin";
//    static readonly string ORIGINAL_FILE_NAME = "arqMovies.txt";

//    class Movie {
//        public long idMovie { get; set; }
//        public string Title { get; set; }
//        public int ReleaseYear { get; set; }
//        public string Url { get; set; }
//        public long idDirector { get; set; }
//        public int Elo { get; set; } = -1;

//        public Movie(string[] Row) {
//            if (Row.Length == 0 || string.IsNullOrEmpty(Row[0])) { idMovie = 0; } else { idMovie = long.Parse(Row[0]); }
//            if (Row.Length < 3 || string.IsNullOrEmpty(Row[2])) { ReleaseYear = 0; } else { ReleaseYear = int.Parse(Row[2]); }
//            if (Row.Length < 5 || string.IsNullOrEmpty(Row[4])) { idDirector = 0; } else { idDirector = long.Parse(Row[4]); }
//            if (Row.Length < 2) { Title = string.Empty; } else { Title = Row[1]; }
//            if (Row.Length < 4) { Url = string.Empty; } else { Url = Row[3]; }
//        }

//        public Movie(long idMovie, string Title, int ReleaseYear, string Url, long idDirector) {
//            this.idMovie = idMovie;
//            this.Title = Title;
//            this.ReleaseYear = ReleaseYear;
//            this.Url = Url;
//            this.idDirector = idDirector;
//        }


//        public void Pad() {
//            Title = Title.PadRight(STRING_SIZE);
//            Url = Url.PadRight(STRING_SIZE);
//        }

//        public Movie(FileStream Fs) {
//            byte[] idBuffer = Fs.ReadBytes(8);
//            byte[] TitleBuffer = Fs.ReadBytes(STRING_SIZE);
//            byte[] ReleaseYearBuffer = Fs.ReadBytes(4);
//            byte[] UrlBuffer = Fs.ReadBytes(STRING_SIZE);
//            byte[] idDirectorBuffer = Fs.ReadBytes(8);
//            byte[] eloBuffer = Fs.ReadBytes(4);

//            idMovie = BitConverter.ToInt64(idBuffer);
//            Title = Encoding.Default.GetString(TitleBuffer).Trim();
//            ReleaseYear = BitConverter.ToInt32(ReleaseYearBuffer);
//            Url = Encoding.Default.GetString(UrlBuffer).Trim();
//            idDirector = BitConverter.ToInt64(idDirectorBuffer);
//            Elo = BitConverter.ToInt32(eloBuffer);
//        }

//        public Movie(byte[] bytes) {
//            byte[] idBuffer = bytes.Take(8).ToArray();
//            bytes = bytes.Skip(8).ToArray();
//            byte[] TitleBuffer = bytes.Take(STRING_SIZE).ToArray();
//            bytes = bytes.Skip(STRING_SIZE).ToArray();
//            byte[] ReleaseYearBuffer = bytes.Take(4).ToArray();
//            bytes = bytes.Skip(4).ToArray();
//            byte[] UrlBuffer = bytes.Take(STRING_SIZE).ToArray();
//            bytes = bytes.Skip(STRING_SIZE).ToArray();
//            byte[] idDirectorBuffer = bytes.Take(8).ToArray();
//            bytes = bytes.Skip(8).ToArray();
//            byte[] eloBuffer = bytes.Take(4).ToArray();


//            idMovie = BitConverter.ToInt64(idBuffer);
//            Title = Encoding.Default.GetString(TitleBuffer).Trim();
//            ReleaseYear = BitConverter.ToInt32(ReleaseYearBuffer);
//            Url = Encoding.Default.GetString(UrlBuffer).Trim();
//            idDirector = BitConverter.ToInt64(idDirectorBuffer);
//            Elo = BitConverter.ToInt32(eloBuffer);
//        }
//    }

//    class IndexObject<T> {
//        public int ContentSize { get; set; }
//        public T Content { get; set; }
//        public int Index { get; set; }
//        public bool ExtensionArea { get; set; }
//    }

//    //[ Extend FileStream ]
//    static byte[] ReadBytes(this FileStream Fs, int nBytes) {
//        byte[] bytes = new byte[nBytes];
//        for (int i = 0; i < nBytes; i++) {
//            bytes[i] = (byte)Fs.ReadByte();
//        }
//        return bytes;
//    }

//    static void Write(this FileStream Fs, Movie movie) {
//        movie.Pad();
//        byte[] idBuffer = BitConverter.GetBytes(movie.idMovie);
//        byte[] TitleBuffer = Encoding.ASCII.GetBytes(movie.Title);
//        byte[] ReleaseYearBuffer = BitConverter.GetBytes(movie.ReleaseYear);
//        byte[] UrlBuffer = Encoding.ASCII.GetBytes(movie.Url);
//        byte[] idDirectorBuffer = BitConverter.GetBytes(movie.idDirector);
//        byte[] eloBuffer = BitConverter.GetBytes(movie.Elo);

//        Fs.Write(idBuffer);
//        Fs.Write(TitleBuffer);
//        Fs.Write(ReleaseYearBuffer);
//        Fs.Write(UrlBuffer);
//        Fs.Write(idDirectorBuffer);
//        Fs.Write(eloBuffer);
//    }

//    static void Write<T>(this FileStream Fs, IndexObject<T> indexObject, StreamWriter? DebugSW = null) {
//        byte[] ContentBuffer = new byte[indexObject.ContentSize];
//        byte[] IndexBuffer = BitConverter.GetBytes(indexObject.Index);
//        byte[] ExtensionBuffer = BitConverter.GetBytes(indexObject.ExtensionArea);


//        if (typeof(T) == typeof(string)) {
//            string Content = indexObject.Content!.ToString()!.PadRight(STRING_SIZE);
//            ContentBuffer = Encoding.ASCII.GetBytes(Content);
//        } else
//        if (typeof(T) == typeof(long)) {
//            long Content = (long)Convert.ChangeType(indexObject.Content, typeof(long))!;
//            ContentBuffer = BitConverter.GetBytes(Content);
//        } else
//        if (typeof(T) == typeof(int)) {
//            int Content = (int)Convert.ChangeType(indexObject.Content, typeof(int))!;
//            ContentBuffer = BitConverter.GetBytes(Content);
//        }

//        Fs.Write(ContentBuffer);
//        Fs.Write(IndexBuffer);
//        Fs.Write(ExtensionBuffer);
//    }
//}
//}