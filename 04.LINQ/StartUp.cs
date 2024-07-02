using System.Diagnostics;
using System.Text;
using Microsoft.EntityFrameworkCore.SqlServer.Storage.Internal;

namespace MusicHub
{
    using System;

    using Data;
    using Initializer;

    public class StartUp
    {
        public static void Main()
        {
            MusicHubDbContext context =
                new MusicHubDbContext();

            DbInitializer.ResetDatabase(context);

            //Console.WriteLine(ExportAlbumsInfo(context, 9));
            Console.WriteLine(ExportSongsAboveDuration(context, 4));
        }

        public static string ExportAlbumsInfo(MusicHubDbContext context, int producerId)
        {
            StringBuilder sb = new StringBuilder();

            var albums = context.Albums
                .Where(a => a.ProducerId == producerId)
                .Select(a => new
                {
                    a.Name,
                    ReleaseDate = a.ReleaseDate.ToString("MM/dd/yyyy"),
                    ProducerName = a.Producer.Name,
                    Songs = a.Songs.Select(s => new
                        {
                            s.Name,
                            s.Price,
                            WriterName = s.Writer.Name,
                        })
                        .OrderByDescending(s => s.Name)
                        .ThenBy(s => s.WriterName)
                        .ToList(),
                    TotalPrice = a.Songs.Sum(s => s.Price)
                })
                .OrderByDescending(a => a.TotalPrice)
                .ToList();

            foreach (var album in albums)
            {
                int count = 1;
                sb.AppendLine($"-AlbumName: {album.Name}");
                sb.AppendLine($"-ReleaseDate: {album.ReleaseDate}");
                sb.AppendLine($"-ProducerName: {album.ProducerName}");
                sb.AppendLine("-Songs:");

                foreach (var song in album.Songs)
                {
                    sb.AppendLine($"---#{count++}");
                    sb.AppendLine($"---SongName: {song.Name}");
                    sb.AppendLine($"---Price: {song.Price:F2}");
                    sb.AppendLine($"---Writer: {song.WriterName}");
                }

                sb.AppendLine($"-AlbumPrice: {album.TotalPrice:F2}");

            }


            return sb.ToString().TrimEnd();
        }

        public static string ExportSongsAboveDuration(MusicHubDbContext context, int duration)
        {

            StringBuilder sb = new();

            var songs = context.Songs
                .AsEnumerable()
                .Where(s => s.Duration.TotalSeconds  > duration)
                .Select(s => new
                {
                    s.Name,
                    PerformerNames = s.SongPerformers.Select(sp => sp.Performer.FirstName + ' ' + sp.Performer.LastName)
                        .OrderBy(p => p)
                        .ToArray(),
                    WriterName = s.Writer.Name,
                    AlbumProducer = s.Album.Producer.Name,
                    Duration = s.Duration.ToString("c")
                })
                .OrderBy(s => s.Name)
                .ThenBy(s => s.WriterName)
                .ToList();

            int songNum = 1;
            foreach (var song in songs)
            {
                sb.AppendLine($"-Song #{songNum++}");
                sb.AppendLine($"---SongName: {song.Name}");
                sb.AppendLine($"---Writer: {song.WriterName}");
                foreach (var p in song.PerformerNames)
                {
                    sb.AppendLine($"---Performer: {p}");
                }
                if (song.PerformerNames.Length > 0)
                {
                }

                sb.AppendLine($"---AlbumProducer: {song.AlbumProducer}");
                sb.AppendLine($"---Duration: {song.Duration}");
            }

            return sb.ToString().TrimEnd();
        }
    }
}
