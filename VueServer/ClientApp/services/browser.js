import axios from '../axios'

const DownloadProtectedFileUrl = `api/directory/download`;
const LoadDirectoryUrl = `api/directory/load`;

export default {
    //download(fn, fol) {
    //    return axios.get(DownloadProtectedFileUrl, {
    //        params: {
    //            fileName: fn,
    //            folder: fol,
    //        }
    //    });
    //},
    loadDirectory(dir, subDir) {
        return axios.get(LoadDirectoryUrl, {
            params: {
                directory: dir,
                subDirectory: subDir,
            }
        });
    },
}
