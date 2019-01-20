const path = require('path');
const webpack = require('webpack');
const OptimizeCSSPlugin = require('optimize-css-assets-webpack-plugin');
const MiniCssExtractPlugin = require("mini-css-extract-plugin");
const { VueLoaderPlugin } = require('vue-loader');

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
            filename: '[name].js',
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
                    test: /\.(sa|sc|c)ss$/,
                    use: isDevBuild ? ['style-loader', 'css-loader'] : [
                            MiniCssExtractPlugin.loader,
                            'css-loader',
                            'sass-loader'
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
                filename: '[name].css',
                //chunkFilename: '[id].css'
            }),

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
