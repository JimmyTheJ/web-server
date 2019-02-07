'use strict'

const path = require('path');
const webpack = require('webpack');
const HtmlPlugin = require('html-webpack-plugin');
const OptimizeCSSPlugin = require('optimize-css-assets-webpack-plugin');
const MiniCssExtractPlugin = require("mini-css-extract-plugin");
const { VueLoaderPlugin } = require('vue-loader');
const VuetifyLoaderPlugin = require('vuetify-loader/lib/plugin')


const bundleOutputDir = './wwwroot/dist';
const devConfig = require('./ClientApp/config/dev.env');
const prodConfig = require('./ClientApp/config/prod.env');

module.exports = (env) => {
    const isDevBuild = !(env && env.prod);
    return [{
        mode: isDevBuild ? 'development' : 'production',
        entry: {
            'main': [
                './ClientApp/boot-app.js',
                './ClientApp/vendor.js'
            ]

        },
        output: {
            filename: isDevBuild ? '[name].js' : '[name].[chunkhash].js',
            path: path.resolve(__dirname, bundleOutputDir),
            publicPath: '/dist/'
        },
        resolve: {
            extensions: ['.js', '.vue'],
            alias: {
                'vue$': 'vue/dist/vue',
                'axios$': path.resolve(__dirname, 'node_modules', 'axios'),
                'components': path.resolve(__dirname, './ClientApp/components'),
            },
            modules: [
                path.resolve('./ClientApp'),
                path.resolve('./node_modules'),
            ],
        },
        watch: true,
        watchOptions: {
            aggregateTimeout: 300,
            poll: 1000,
            ignored: /node_modules/
        },
        optimization: {
            minimize: isDevBuild ? false : true,
            runtimeChunk: {
                name: 'main',
            },
            splitChunks: {
                cacheGroups: {
                    vendor: {
                        test: /[\\/]node_modules[\\/]/,
                        name: 'vendor',
                        enforce: true,
                        chunks: 'all',
                    },
                }
            }
        },
        module: {
            rules: [
                {
                    test: /\.vue$/,
                    loader: 'vue-loader',
                    options: {
                        loaders: {
                            js: 'babel-loader',
                            //scss: 'vue-style-loader!css-loader!sass-loader'
                        },
                        options: {
                            babelrc: true
                        },
                    },
                },

                {
                    test: /\.js$/,
                    //include: [/ClientApp/, path.resolve('node_modules/vue-awesome'), path.resolve('node_modules/vue-loader')],
                    loader: 'babel-loader',
                    options: {
                        babelrc: true
                    }
                },

                {
                    test: /\.s(c|a)ss$/,
                    use: [
                        //MiniCssExtractPlugin.loader,
                        'vue-style-loader',
                        'css-loader',
                        {
                            loader: 'sass-loader',
                            options: {
                                implementation: require('sass'),
                                sassOptions: {
                                    fiber: require('fibers'),
                                    indentedSyntax: true // optional
                                },
                            },
                        },
                    ],
                },
                {
                    test: /\.css$/,
                    use: [
                        MiniCssExtractPlugin.loader,
                        'css-loader',
                    ]
                },
                {
                    test: /\.(png|jpg|jpeg|gif|svg|ttf|eot|woff(2)?)$/,
                    use: 'url-loader?limit=25000'
                }
            ]
        },
        plugins: [
            new MiniCssExtractPlugin({
                filename: isDevBuild ? '[name].css' : '[name].[chunkhash].css',
                //chunkFilename: '[id].css'
            }),

            // Rewrite index.html based off template and with new hashes
            new HtmlPlugin({
                //filename: '',
                title: 'Dotnet Core Vue.js Web server',
                chunks: {
                    main: {
                        entry: 'main.js',
                        css: [ 'main.css' ]
                    },
                    vendor: {
                        entry: 'vendor.js',
                        css: [ 'vendor.css' ]
                    }
                },
                template: 'ClientApp/index.html'
            }),

            new VuetifyLoaderPlugin(),
            new VueLoaderPlugin(),
        ].concat(isDevBuild ? [
            // Plugins that apply in development builds only
            new webpack.DefinePlugin({
                'process.env': devConfig
            }),

        ] : [
                // Plugins that apply in production builds only
                new webpack.DefinePlugin({
                    'process.env': prodConfig
                }),


        ])
    }]
}
