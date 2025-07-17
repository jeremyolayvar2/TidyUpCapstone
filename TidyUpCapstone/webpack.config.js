const path = require('path');

module.exports = {
    mode: 'development',
    entry: './Scripts/reactApp.jsx',
    output: {
        path: path.resolve(__dirname, 'wwwroot/js'),
        filename: 'bundle.js',
    },
    module: {
        rules: [
            {
                test: /\.jsx?$/,
                exclude: /node_modules/,
                use: {
                    loader: 'babel-loader',
                }
            }
        ]
    },
    resolve: {
        extensions: ['.js', '.jsx'],
    },
    devtool: 'source-map'
};
