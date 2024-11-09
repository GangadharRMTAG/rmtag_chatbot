import queueMicrotask from 'queue-microtask';
(window as any).queueMicrotask = queueMicrotask;


(window as any).global = window;

(function() {
    if (typeof globalThis === 'undefined') {
        Object.defineProperty(Object.prototype, 'globalThis', {
            get: function () {
                return this;
            },
            configurable: true
        });
    }
})();



import './zone-flags';

import 'zone.js';  // Included with Angular CLI.

import 'globalthis/auto';
import 'array-flat-polyfill';

import 'globalthis/polyfill';

import 'core-js/es/global-this'; 
import 'core-js/es/symbol'; 
import 'core-js/proposals/global-this';




import 'core-js/es/object';
import 'core-js/es/function';
import 'core-js/es/parse-int';
import 'core-js/es/parse-float';
import 'core-js/es/number';
import 'core-js/es/math';
import 'core-js/es/string';
import 'core-js/es/date';
import 'core-js/es/array';
import 'core-js/es/regexp';
import 'core-js/es/map';
import 'core-js/es/set';
import 'core-js/es/weak-map';
import 'core-js/es/weak-set';
import 'core-js/es/promise';





  

