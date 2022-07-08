using BingsuCodeEditor.AutoCompleteToken;
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
        public abstract string GetFIleContent(string pullpath);


        /// <summary>
        /// 파일이 캐시되어있는지 확인
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public bool IsCachedContainer(string pullpath)
        {
            return CachedContainer.Keys.Contains(pullpath);
        }


        //파일이 변형되지 않았을 경우 여기서 가져옴.
        private Dictionary<string, Container> CachedContainer = new Dictionary<string, Container>();

        /// <summary>
        /// 콘테이너를 가져오는 함수
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public void UpdateContainer(string pullpath, Container container)
        {
            if (CachedContainer.Keys.Contains(pullpath))
            {
                CachedContainer[pullpath] = container;
            }
            else
            {
                CachedContainer.Add(pullpath, container);
            }
        }


        /// <summary>
        /// 콘테이너를 가져오는 함수
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public Container GetContainer(string pullpath)
        {
            return CachedContainer[pullpath];
        }

     



        /// <summary>
        /// 파일이 존재하는지 확인하는 함수
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public bool IsFileExist(string filename, string basefilename = "")
        {
            return GetFileList(basefilename).IndexOf(filename) != -1;
        }

        /// <summary>
        /// 현재 존재하는 모든 파일들을 가져옵니다.
        /// </summary>
        /// <param name="filename">현재 파일의 이름입니다.</param>
        /// <returns></returns>
        public abstract List<string> GetFileList(string basefilename = "");


        /// <summary>
        /// 파일의 절대 주소를 구합니다.
        /// </summary>
        /// <param name="filename">현재 파일의 이름입니다.</param>
        /// <param name="basefilename">파일이 들어있는 폴더</param>
        /// <returns></returns>
        public string GetPullPath(string filename, string basefilename = "")
        {
            List<string> flist = GetFileList();

            string connectname = basefilename + "." + filename;

            int i = flist.IndexOf(connectname);
            if(i != -1)
            {
                return flist[i];
            }

            i = flist.IndexOf(filename);
            if (i != -1)
            {
                return flist[i];
            }



            return "";
        }


        /// <summary>
        /// 파일의 절대 폴더를 구합니다..
        /// </summary>
        /// <param name="filename">현재 파일의 이름입니다.</param>
        /// <param name="basefilename">파일이 들어있는 폴더</param>
        /// <returns></returns>
        public string GetFloderPath(string filename, string basefilename = "")
        {
            string t = GetPullPath(filename, basefilename);
            t = t.Substring(0, t.LastIndexOf("."));
            return t;
        }


        public abstract string GetDefaultFunctions();
    }
}
