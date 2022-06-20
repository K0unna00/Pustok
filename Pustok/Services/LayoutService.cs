using Pustok.DAL;
using Pustok.Models;
using System.Collections.Generic;
using System.Linq;

namespace Pustok.Services
{
    public class LayoutService
    {
        private readonly AppDbContext _context;

        public LayoutService(AppDbContext context )
        {
            _context = context;
        }
        public List<Genre> GetGenre()
        {
            return _context.Genres.ToList();
        }
        public Dictionary<string,string> GetSetting()
        {
            return _context.Settings.ToDictionary( x => x.Key, x => x.Value );
        }
    }
}
