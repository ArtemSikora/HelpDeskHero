window.authStorage = {

    getToken: function () {
        return localStorage.getItem(
            "hdh_token");
    },

    setToken: function (value) {
        localStorage.setItem(
            "hdh_token",
            value);
    },

    removeToken: function () {
        localStorage.removeItem(
            "hdh_token");
    },

    getRefreshToken: function () {
        return localStorage.getItem(
            "hdh_refresh_token");
    },

    setRefreshToken: function (value) {
        localStorage.setItem(
            "hdh_refresh_token",
            value);
    },

    removeRefreshToken: function () {
        localStorage.removeItem(
            "hdh_refresh_token");
    },

    getRole: function () {
        return localStorage.getItem(
            "hdh_role");
    },

    setRole: function (value) {
        localStorage.setItem(
            "hdh_role",
            value);
    },

    removeRole: function () {
        localStorage.removeItem(
            "hdh_role");
    },

    downloadTextFile: function (fileName, contentType, content) {
        const blob = new Blob(
            [content],
            { type: contentType });

        const url = URL.createObjectURL(
            blob);

        const anchor = document.createElement(
            "a");

        anchor.href = url;
        anchor.download = fileName;
        anchor.click();

        URL.revokeObjectURL(
            url);
    }
};
