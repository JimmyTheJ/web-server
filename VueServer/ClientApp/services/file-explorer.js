import axios from '../axios'

const DownloadProtectedFileUrl = `download/file`;
const LoadDirectoryUrl = `/load/folder`;
const FileListUrl = `api/directory/list`;

export default {
    //download(fn, fol) {
    //    return axios.get(DownloadProtectedFileUrl, {
    //        params: {
    //            fileName: fn,
    //            folder: fol,
    //        }
    //    });
    //},
    loadDirectory(dir, subDir, level) {
        if (!subDir)
            return axios.get(`${LoadDirectoryUrl}/${level}/${dir}`);
        else
            return axios.get(`${LoadDirectoryUrl}/${level}/${dir}?dir=${subDir}`);
    },
    getFolderList(lvl) {
        return axios.get(FileListUrl, {
            params: {
                level: lvl,
            },
        });
    },
}
