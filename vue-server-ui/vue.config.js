// vue.config.js
const webpack = require('webpack');
const HtmlPlugin = require('html-webpack-plugin');

let devConfig;
let prodConfig;

// Get configs, but fall back to default config if they aren't present
try {
  devConfig = require('./src/config/config.dev.env');	
}
catch (ex) {
  console.log('No development config file available, loading the default');
  devConfig = require('./src/config/config.env');	
}
try {
  prodConfig = require('./src/config/config.prod.env');
}
catch (ex) {
  console.log('No production config file available, loading the default');
  prodConfig = require('./src/config/config.env');	
}
// End get configs

module.exports = (env) => {
  const isDevBuild = !(env && env.prod);
  
  return {
    configureWebpack: {
      plugins: [
        // // Rewrite index.html based off template and with new hashes
        // new HtmlPlugin({
        //   //filename: '',
        //   title: 'Dotnet Core Vue.js Web server',
        //   chunks: {
        //     main: {
        //       entry: 'main.js',
        //       css: [ 'main.css' ]
        //     },
        //     vendor: {
        //       entry: 'vendor.js',
        //       css: [ 'vendor.css' ]
        //     }
        //   },
        //   template: 'src/index.html'
        // })
      ].concat(isDevBuild ? [
        // Plugins that apply in development builds only
        new webpack.DefinePlugin({
          'process.env': devConfig
        })
      ] : [
        // Plugins that apply in production builds only
        new webpack.DefinePlugin({
          'process.env': prodConfig
        })
      ])
    },
    
    chainWebpack: config => {
      config.module.rules.delete('eslint');
    },

    transpileDependencies: [
      'vuetify'
    ]
  }
}
