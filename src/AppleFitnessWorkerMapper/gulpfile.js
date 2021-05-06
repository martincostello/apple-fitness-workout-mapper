var browserify = require('browserify');
var buffer = require('vinyl-buffer');
var gulp = require('gulp');
var source = require('vinyl-source-stream');
var sourcemaps = require('gulp-sourcemaps');
var tsify = require('tsify');
var tslint = require('gulp-tslint');
var uglify = require('gulp-uglify');

gulp.task('lint', function () {
  return gulp.src('scripts/ts/**/*.ts')
    .pipe(tslint({
      formatter: 'msbuild'
    }))
    .pipe(tslint.report());
});

gulp.task('build', function () {
  return browserify({
    basedir: '.',
    debug: true,
    entries: ['scripts/ts/main.ts'],
    cache: {},
    packageCache: {}
  })
    .plugin(tsify)
    .transform('babelify', {
      presets: ['es2015'],
      extensions: ['.ts'],
      plugins: [
        [
          "transform-runtime",
          {
            "polyfill": false,
            "regenerator": true
          }
        ]
      ]
    })
    .bundle()
    .pipe(source('main.js'))
    .pipe(buffer())
    .pipe(sourcemaps.init({ loadMaps: true }))
    .pipe(uglify())
    .pipe(sourcemaps.write('./'))
    .pipe(gulp.dest('wwwroot/static/js'));
});

gulp.task('default', gulp.series('lint', 'build'));
