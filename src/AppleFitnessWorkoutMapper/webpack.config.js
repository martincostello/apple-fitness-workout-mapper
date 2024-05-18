const path = require('path');
const webpack = require('webpack');

module.exports = {
    devtool: 'source-map',
    entry: './scripts/ts/main.ts',
    mode: 'production',
    module: {
        rules: [
            {
                test: /\.tsx?$/,
                use: 'ts-loader',
                exclude: /node_modules/,
            },
        ],
    },
    output: {
        filename: 'main.js',
        path: path.resolve(__dirname, 'wwwroot', 'static', 'js'),
    },
    plugins: [
        new webpack.ContextReplacementPlugin(/moment[/\\]locale$/, /en-gb/),
    ],
    resolve: {
        extensions: ['.tsx', '.ts', '.js'],
    },
};
