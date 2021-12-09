using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BingsuCodeEditor
{
    public abstract class ImportManager
    {
        /// <summary>
        /// 파일의 내용을 가져오는 함수
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public abstract string GetFIleContent(string filename);

        /// <summary>
        /// 파일이 존재하는지 확인하는 함수
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public abstract bool IsFileExist(string filename);

        public abstract string GetDefaultFunctions();
    }
}
