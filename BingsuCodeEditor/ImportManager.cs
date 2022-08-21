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
        /// 기본 함수 파일들을 가져오는 함수
        /// </summary>
        /// <returns></returns>
        public abstract List<string> GetFIleList();


        public CodeTextEditor.CodeType CodeType;

        /// <summary>
        /// 파일이 캐시되어있는지 확인
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public bool IsCachedContainer(string pullpath)
        {
            return CachedContainer.Keys.Contains(pullpath);
        }


        /// <summary>
        /// 파일이 캐시되어있는지 확인
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public void CachedContainerRemove(string pullpath)
        {
            if (CachedContainer.Keys.Contains(pullpath))
            {
                //존재할 경우
                CachedContainer.Remove(pullpath);
            }
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
            return GetImportedFileList(basefilename).IndexOf(filename) != -1;
        }

        /// <summary>
        /// 현재 존재하는 모든 파일들을 가져옵니다.
        /// </summary>
        /// <param name="filename">현재 파일의 이름입니다.</param>
        /// <returns></returns>
        public abstract List<string> GetImportedFileList(string basefilename = "");


        /// <summary>
        /// 파일의 절대 주소를 구합니다.
        /// </summary>
        /// <param name="filename">현재 파일의 이름입니다.</param>
        /// <param name="basefilename">파일이 들어있는 폴더</param>
        /// <returns></returns>
        public string GetPullPath(string filename, string basefilename = "")
        {
            char spliter = '\'';
            string redo = "..";


            List<string> flist = GetImportedFileList();


            //연결 했을때 풀네임
            //filenamedㅣ ..이 들어간다면 basefilename에서 제거해야됨.
            List<string> btlist = basefilename.Split('.').ToList();
            string f = filename.Replace(redo, "/" + spliter);
            List<string> ftlist = f.Split(spliter).ToList();

            while(ftlist.Count > 0)
            {
                string fstr = ftlist.First();
                ftlist.RemoveAt(0);

                //Back문자일경우 bt에서 제거
                if (fstr == "/")
                {
                    if(btlist.Count == 0)
                    {
                        return "";
                    }
                    btlist.RemoveAt(btlist.Count - 1);
                }
                else
                {
                    //아닐 경우 추가
                    btlist.Add(fstr);
                }
            }

            string connectname = "";

            for (int j = 0; j < btlist.Count; j++)
            {
                if(j != 0) connectname += ".";
                connectname += btlist[j];
            }

            int i = flist.IndexOf(connectname);
            if(i != -1) return flist[i];


            //그 자체로 풀네임
            i = flist.IndexOf(filename);
            if (i != -1) return flist[i];

            return "";
        }





        public abstract string GetDefaultFunctions();
    }
}
