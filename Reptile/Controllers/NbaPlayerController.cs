using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Service.Interfaces;

namespace Reptile.Controllers
{
    /// <summary>
    /// 下載Nba球員生涯資料
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class NbaPlayerController : ControllerBase
    {
        private readonly INbaPlayerService _nbaPlayService;

        /// <summary>
        /// NbaPlayerController
        /// </summary>
        /// <param name="nbaPlayService"></param>
        public NbaPlayerController(INbaPlayerService nbaPlayService)
        {
            this._nbaPlayService = nbaPlayService;
        }

        /// <summary>
        /// 取得Nba球員生涯資料壓縮檔
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> GetAsync()
        {
            var fileDto = await this._nbaPlayService.GetAsync();

            var fr = this.File(fileDto, "application/zip", "NbaPlayerCareer.zip");

            return fr;
        }
    }
}