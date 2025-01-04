using SQLite;
using static DenemeKupon.Harita1;

namespace DenemeKupon
{
    public class KuponVeritabani
    {
        private readonly SQLiteAsyncConnection _db;

        public KuponVeritabani(string dbPath)
        {
            _db = new SQLiteAsyncConnection(dbPath);
            _db.CreateTableAsync<Kupon>().Wait();
        }

        public Task<List<Kupon>> GetKuponsAsync()
        {
            return _db.Table<Kupon>().ToListAsync();
        }

        public Task<int> SaveKuponAsync(Kupon kupon)
        {
            return _db.InsertAsync(kupon);
        }

        public Task<int> DeleteKuponAsync(Kupon kupon)
        {
            return _db.DeleteAsync(kupon);
        }
    }
}