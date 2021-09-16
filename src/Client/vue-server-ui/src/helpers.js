export const getNewBookcase = function() {
  return {
    id: 0,
    name: '',
  }
}

export const getNewSeries = function() {
  return {
    id: 0,
    name: '',
    number: 0,
    active: false,
  }
}

export const getNewShelf = function() {
  return {
    id: 0,
    name: '',
  }
}

export const getNewAuthor = function() {
  return {
    id: 0,
    firstName: null,
    lastName: null,
    fullName: null,
    deceased: false,
  }
}

export const padTwo = function(number) {
  return (number < 10 ? '0' : '') + number
}

export const convertPemissionsToFlags = function(obj) {
  let clone = Object.assign({}, obj)
  if (
    Array.isArray(clone.selectedPermissions) &&
    clone.selectedPermissions.length > 0
  ) {
    clone.accessFlags = 0
    clone.selectedPermissions.forEach(value => {
      clone.accessFlags += value
    })
    delete clone.selectedPermissions

    return clone
  } else {
    if (typeof clone.selectedPermissions !== 'undefined')
      delete clone.selectedPermissions
    return clone
  }
}

export const convertFlagsToPermissions = function(obj) {
  obj.selectedPermissions = []
  let i = 0
  let num = Math.pow(2, 8)

  if (obj.accessFlags > 0) {
    let val = obj.accessFlags
    while (num > 0) {
      if (val / num >= 1) {
        obj.selectedPermissions.push({ key: i++, value: num })
        val -= num
      }
      num /= 2
    }
  }

  return obj
}
