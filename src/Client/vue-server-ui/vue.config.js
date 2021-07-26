// vue.config.js
const Dotenv = require('dotenv-webpack')
const Html = require('html-webpack-plugin')

module.exports = {
  configureWebpack: {
    plugins: [
      new Dotenv(),
      new Html({
        template: 'public/index.html',
        title: process.env.VUE_APP_TITLE,
      }),
    ],
  },

  chainWebpack: config => {
    config.module.rules.delete('eslint')
  },

  transpileDependencies: ['vuetify'],
}
