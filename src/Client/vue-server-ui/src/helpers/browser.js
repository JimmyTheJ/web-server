import { MediaTypes } from '@/constants'

export const getSubdirectoryString = function(subDirs) {
  let path = ''

  if (!Array.isArray(subDirs)) {
    return path
  }

  for (let i = 0; i < subDirs.length; i++) {
    path += subDirs[i]

    if (subDirs.length - 1 !== i) {
      path += '/'
    }
  }

  return path
}

export const getSubdirectoryArray = function(subDirString) {
  let obj = subDirString
  let objArray = []

  if (
    typeof subDirString === 'undefined' ||
    subDirString === null ||
    subDirString === ''
  ) {
    return objArray
  }

  while (obj.includes('/')) {
    const tmpPiece = obj.substr(0, obj.indexOf('/'))

    if (tmpPiece !== '') objArray.push(tmpPiece)

    obj = obj.substr(tmpPiece.length + 1)
  }
  objArray.push(obj)

  return objArray
}

export const splitPathFromRoute = function(path) {
  let obj = null
  if (typeof path === 'undefined' || path === null || path === '') {
    return obj
  }

  if (!path.includes('/')) {
    obj = {
      base: path,
      subDirs: null,
    }
  } else {
    obj = {
      base: path.substr(0, path.indexOf('/')),
      subDirs: path.substr(path.indexOf('/') + 1),
    }
  }

  return obj
}

export const getFileType = function(file) {
  if (typeof file === 'undefined' || file === null) return null
  else {
    switch (file.extension) {
      case '.mkv':
      case '.avi':
      case '.mpeg':
      case '.mpg':
      case '.mp4':
      case '.wmv':
      case '.webm':
      case '.mp3':
        return MediaTypes.Video
      case '.jpg':
      case '.jpeg':
      case '.gif':
      case '.tiff':
      case '.bmp':
      case '.png':
      case '.img':
        return MediaTypes.Image
      default:
        return MediaTypes.Text
    }
  }
}
