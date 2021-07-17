using System.Threading.Tasks;

namespace Service.Interfaces
{
    public interface INbaPlayerService
    {
        /// <summary>
        /// 取得Nba球員生涯資料壓縮檔
        /// </summary>
        /// <returns></returns>
        Task<byte[]> GetAsync();
    }
}