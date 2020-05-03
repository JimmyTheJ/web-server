export const getNewBookcase = function () {
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
        deceased: false
    }
}
