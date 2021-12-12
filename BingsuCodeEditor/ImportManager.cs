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

        /// <summary>
        /// 현재 존재하는 모든 파일들을 가져옵니다.
        /// </summary>
        /// <param name="filename">현재 파일의 이름입니다.</param>
        /// <returns></returns>
        public abstract List<string> GetFileList(string filename = "");

        public abstract string GetDefaultFunctions();
    }
}
