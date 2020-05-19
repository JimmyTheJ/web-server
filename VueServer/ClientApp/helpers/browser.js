export const getSubdirectoryString = function (subDirs) {
    if (!Array.isArray(subDirs)) {
        return null;
    }

    let path = '';
    for (let i = 0; i < subDirs.length; i++) {
        path += subDirs[i];

        if (subDirs.length - 1 !== i) {
            path += '/';
        }
    }

    return path;
}

export const getSubdirectoryArray = function (subDirString) {
    let obj = subDirString;
    let objArray = [];

    if (typeof subDirString === 'undefined' || subDirString === null || subDirString === '') {
        return objArray;
    }        

    while (obj.includes("/")) {
        const tmpPiece = obj.substr(0, obj.indexOf("/"));

        objArray.push(tmpPiece);

        obj = obj.substr(tmpPiece.length + 1);
    }
    objArray.push(obj);

    return objArray;
}
