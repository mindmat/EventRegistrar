/******/ (function(modules) { // webpackBootstrap
/******/ 	// The module cache
/******/ 	var installedModules = {};
/******/
/******/ 	// The require function
/******/ 	function __webpack_require__(moduleId) {
/******/
/******/ 		// Check if module is in cache
/******/ 		if(installedModules[moduleId]) {
/******/ 			return installedModules[moduleId].exports;
/******/ 		}
/******/ 		// Create a new module (and put it into the cache)
/******/ 		var module = installedModules[moduleId] = {
/******/ 			i: moduleId,
/******/ 			l: false,
/******/ 			exports: {}
/******/ 		};
/******/
/******/ 		// Execute the module function
/******/ 		modules[moduleId].call(module.exports, module, module.exports, __webpack_require__);
/******/
/******/ 		// Flag the module as loaded
/******/ 		module.l = true;
/******/
/******/ 		// Return the exports of the module
/******/ 		return module.exports;
/******/ 	}
/******/
/******/
/******/ 	// expose the modules object (__webpack_modules__)
/******/ 	__webpack_require__.m = modules;
/******/
/******/ 	// expose the module cache
/******/ 	__webpack_require__.c = installedModules;
/******/
/******/ 	// identity function for calling harmony imports with the correct context
/******/ 	__webpack_require__.i = function(value) { return value; };
/******/
/******/ 	// define getter function for harmony exports
/******/ 	__webpack_require__.d = function(exports, name, getter) {
/******/ 		if(!__webpack_require__.o(exports, name)) {
/******/ 			Object.defineProperty(exports, name, {
/******/ 				configurable: false,
/******/ 				enumerable: true,
/******/ 				get: getter
/******/ 			});
/******/ 		}
/******/ 	};
/******/
/******/ 	// getDefaultExport function for compatibility with non-harmony modules
/******/ 	__webpack_require__.n = function(module) {
/******/ 		var getter = module && module.__esModule ?
/******/ 			function getDefault() { return module['default']; } :
/******/ 			function getModuleExports() { return module; };
/******/ 		__webpack_require__.d(getter, 'a', getter);
/******/ 		return getter;
/******/ 	};
/******/
/******/ 	// Object.prototype.hasOwnProperty.call
/******/ 	__webpack_require__.o = function(object, property) { return Object.prototype.hasOwnProperty.call(object, property); };
/******/
/******/ 	// __webpack_public_path__
/******/ 	__webpack_require__.p = "dist/";
/******/
/******/ 	// Load entry module and return exports
/******/ 	return __webpack_require__(__webpack_require__.s = 15);
/******/ })
/************************************************************************/
/******/ ([
/* 0 */
/***/ (function(module, exports) {

module.exports = vendor_4adf5b975b06d7f766a2;

/***/ }),
/* 1 */
/***/ (function(module, exports, __webpack_require__) {

module.exports = (__webpack_require__(0))(3);

/***/ }),
/* 2 */
/***/ (function(module, exports) {

/*
	MIT License http://www.opensource.org/licenses/mit-license.php
	Author Tobias Koppers @sokra
*/
// css base code, injected by the css-loader
module.exports = function(useSourceMap) {
	var list = [];

	// return the list of modules as css string
	list.toString = function toString() {
		return this.map(function (item) {
			var content = cssWithMappingToString(item, useSourceMap);
			if(item[2]) {
				return "@media " + item[2] + "{" + content + "}";
			} else {
				return content;
			}
		}).join("");
	};

	// import a list of modules into the list
	list.i = function(modules, mediaQuery) {
		if(typeof modules === "string")
			modules = [[null, modules, ""]];
		var alreadyImportedModules = {};
		for(var i = 0; i < this.length; i++) {
			var id = this[i][0];
			if(typeof id === "number")
				alreadyImportedModules[id] = true;
		}
		for(i = 0; i < modules.length; i++) {
			var item = modules[i];
			// skip already imported module
			// this implementation is not 100% perfect for weird media query combinations
			//  when a module is imported multiple times with different media queries.
			//  I hope this will never occur (Hey this way we have smaller bundles)
			if(typeof item[0] !== "number" || !alreadyImportedModules[item[0]]) {
				if(mediaQuery && !item[2]) {
					item[2] = mediaQuery;
				} else if(mediaQuery) {
					item[2] = "(" + item[2] + ") and (" + mediaQuery + ")";
				}
				list.push(item);
			}
		}
	};
	return list;
};

function cssWithMappingToString(item, useSourceMap) {
	var content = item[1] || '';
	var cssMapping = item[3];
	if (!cssMapping) {
		return content;
	}

	if (useSourceMap && typeof btoa === 'function') {
		var sourceMapping = toComment(cssMapping);
		var sourceURLs = cssMapping.sources.map(function (source) {
			return '/*# sourceURL=' + cssMapping.sourceRoot + source + ' */'
		});

		return [content].concat(sourceURLs).concat([sourceMapping]).join('\n');
	}

	return [content].join('\n');
}

// Adapted from convert-source-map (MIT)
function toComment(sourceMap) {
	// eslint-disable-next-line no-undef
	var base64 = btoa(unescape(encodeURIComponent(JSON.stringify(sourceMap))));
	var data = 'sourceMappingURL=data:application/json;charset=utf-8;base64,' + base64;

	return '/*# ' + data + ' */';
}


/***/ }),
/* 3 */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "a", function() { return AppComponent; });
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_0__angular_core__ = __webpack_require__(1);
var __decorate = (this && this.__decorate) || function (decorators, target, key, desc) {
    var c = arguments.length, r = c < 3 ? target : desc === null ? desc = Object.getOwnPropertyDescriptor(target, key) : desc, d;
    if (typeof Reflect === "object" && typeof Reflect.decorate === "function") r = Reflect.decorate(decorators, target, key, desc);
    else for (var i = decorators.length - 1; i >= 0; i--) if (d = decorators[i]) r = (c < 3 ? d(r) : c > 3 ? d(target, key, r) : d(target, key)) || r;
    return c > 3 && r && Object.defineProperty(target, key, r), r;
};

var AppComponent = (function () {
    function AppComponent() {
    }
    AppComponent = __decorate([
        __webpack_require__.i(__WEBPACK_IMPORTED_MODULE_0__angular_core__["Component"])({
            selector: 'app',
            template: __webpack_require__(19),
            styles: [__webpack_require__(24)]
        })
    ], AppComponent);
    return AppComponent;
}());



/***/ }),
/* 4 */
/***/ (function(module, exports, __webpack_require__) {

module.exports = (__webpack_require__(0))(39);

/***/ }),
/* 5 */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "a", function() { return AppModule; });
/* unused harmony export getBaseUrl */
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_0__angular_core__ = __webpack_require__(1);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_1__angular_platform_browser__ = __webpack_require__(30);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_2__app_module_shared__ = __webpack_require__(10);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_3__components_app_app_component__ = __webpack_require__(3);
var __decorate = (this && this.__decorate) || function (decorators, target, key, desc) {
    var c = arguments.length, r = c < 3 ? target : desc === null ? desc = Object.getOwnPropertyDescriptor(target, key) : desc, d;
    if (typeof Reflect === "object" && typeof Reflect.decorate === "function") r = Reflect.decorate(decorators, target, key, desc);
    else for (var i = decorators.length - 1; i >= 0; i--) if (d = decorators[i]) r = (c < 3 ? d(r) : c > 3 ? d(target, key, r) : d(target, key)) || r;
    return c > 3 && r && Object.defineProperty(target, key, r), r;
};




var AppModule = (function () {
    function AppModule() {
    }
    AppModule = __decorate([
        __webpack_require__.i(__WEBPACK_IMPORTED_MODULE_0__angular_core__["NgModule"])({
            bootstrap: [__WEBPACK_IMPORTED_MODULE_3__components_app_app_component__["a" /* AppComponent */]],
            imports: [
                __WEBPACK_IMPORTED_MODULE_1__angular_platform_browser__["BrowserModule"],
                __WEBPACK_IMPORTED_MODULE_2__app_module_shared__["a" /* AppModuleShared */]
            ],
            providers: [
                { provide: 'BASE_URL', useFactory: getBaseUrl }
            ]
        })
    ], AppModule);
    return AppModule;
}());

function getBaseUrl() {
    return document.getElementsByTagName('base')[0].href;
}


/***/ }),
/* 6 */
/***/ (function(module, exports, __webpack_require__) {

/* WEBPACK VAR INJECTION */(function(process, global) {/*! *****************************************************************************
Copyright (C) Microsoft. All rights reserved.
Licensed under the Apache License, Version 2.0 (the "License"); you may not use
this file except in compliance with the License. You may obtain a copy of the
License at http://www.apache.org/licenses/LICENSE-2.0

THIS CODE IS PROVIDED ON AN *AS IS* BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
KIND, EITHER EXPRESS OR IMPLIED, INCLUDING WITHOUT LIMITATION ANY IMPLIED
WARRANTIES OR CONDITIONS OF TITLE, FITNESS FOR A PARTICULAR PURPOSE,
MERCHANTABLITY OR NON-INFRINGEMENT.

See the Apache Version 2.0 License for specific language governing permissions
and limitations under the License.
***************************************************************************** */
var Reflect;
(function (Reflect) {
    "use strict";
    var hasOwn = Object.prototype.hasOwnProperty;
    // feature test for Symbol support
    var supportsSymbol = typeof Symbol === "function";
    var toPrimitiveSymbol = supportsSymbol && typeof Symbol.toPrimitive !== "undefined" ? Symbol.toPrimitive : "@@toPrimitive";
    var iteratorSymbol = supportsSymbol && typeof Symbol.iterator !== "undefined" ? Symbol.iterator : "@@iterator";
    var HashMap;
    (function (HashMap) {
        var supportsCreate = typeof Object.create === "function"; // feature test for Object.create support
        var supportsProto = { __proto__: [] } instanceof Array; // feature test for __proto__ support
        var downLevel = !supportsCreate && !supportsProto;
        // create an object in dictionary mode (a.k.a. "slow" mode in v8)
        HashMap.create = supportsCreate
            ? function () { return MakeDictionary(Object.create(null)); }
            : supportsProto
                ? function () { return MakeDictionary({ __proto__: null }); }
                : function () { return MakeDictionary({}); };
        HashMap.has = downLevel
            ? function (map, key) { return hasOwn.call(map, key); }
            : function (map, key) { return key in map; };
        HashMap.get = downLevel
            ? function (map, key) { return hasOwn.call(map, key) ? map[key] : undefined; }
            : function (map, key) { return map[key]; };
    })(HashMap || (HashMap = {}));
    // Load global or shim versions of Map, Set, and WeakMap
    var functionPrototype = Object.getPrototypeOf(Function);
    var usePolyfill = typeof process === "object" && process.env && process.env["REFLECT_METADATA_USE_MAP_POLYFILL"] === "true";
    var _Map = !usePolyfill && typeof Map === "function" && typeof Map.prototype.entries === "function" ? Map : CreateMapPolyfill();
    var _Set = !usePolyfill && typeof Set === "function" && typeof Set.prototype.entries === "function" ? Set : CreateSetPolyfill();
    var _WeakMap = !usePolyfill && typeof WeakMap === "function" ? WeakMap : CreateWeakMapPolyfill();
    // [[Metadata]] internal slot
    // https://rbuckton.github.io/reflect-metadata/#ordinary-object-internal-methods-and-internal-slots
    var Metadata = new _WeakMap();
    /**
      * Applies a set of decorators to a property of a target object.
      * @param decorators An array of decorators.
      * @param target The target object.
      * @param propertyKey (Optional) The property key to decorate.
      * @param attributes (Optional) The property descriptor for the target key.
      * @remarks Decorators are applied in reverse order.
      * @example
      *
      *     class Example {
      *         // property declarations are not part of ES6, though they are valid in TypeScript:
      *         // static staticProperty;
      *         // property;
      *
      *         constructor(p) { }
      *         static staticMethod(p) { }
      *         method(p) { }
      *     }
      *
      *     // constructor
      *     Example = Reflect.decorate(decoratorsArray, Example);
      *
      *     // property (on constructor)
      *     Reflect.decorate(decoratorsArray, Example, "staticProperty");
      *
      *     // property (on prototype)
      *     Reflect.decorate(decoratorsArray, Example.prototype, "property");
      *
      *     // method (on constructor)
      *     Object.defineProperty(Example, "staticMethod",
      *         Reflect.decorate(decoratorsArray, Example, "staticMethod",
      *             Object.getOwnPropertyDescriptor(Example, "staticMethod")));
      *
      *     // method (on prototype)
      *     Object.defineProperty(Example.prototype, "method",
      *         Reflect.decorate(decoratorsArray, Example.prototype, "method",
      *             Object.getOwnPropertyDescriptor(Example.prototype, "method")));
      *
      */
    function decorate(decorators, target, propertyKey, attributes) {
        if (!IsUndefined(propertyKey)) {
            if (!IsArray(decorators))
                throw new TypeError();
            if (!IsObject(target))
                throw new TypeError();
            if (!IsObject(attributes) && !IsUndefined(attributes) && !IsNull(attributes))
                throw new TypeError();
            if (IsNull(attributes))
                attributes = undefined;
            propertyKey = ToPropertyKey(propertyKey);
            return DecorateProperty(decorators, target, propertyKey, attributes);
        }
        else {
            if (!IsArray(decorators))
                throw new TypeError();
            if (!IsConstructor(target))
                throw new TypeError();
            return DecorateConstructor(decorators, target);
        }
    }
    Reflect.decorate = decorate;
    // 4.1.2 Reflect.metadata(metadataKey, metadataValue)
    // https://rbuckton.github.io/reflect-metadata/#reflect.metadata
    /**
      * A default metadata decorator factory that can be used on a class, class member, or parameter.
      * @param metadataKey The key for the metadata entry.
      * @param metadataValue The value for the metadata entry.
      * @returns A decorator function.
      * @remarks
      * If `metadataKey` is already defined for the target and target key, the
      * metadataValue for that key will be overwritten.
      * @example
      *
      *     // constructor
      *     @Reflect.metadata(key, value)
      *     class Example {
      *     }
      *
      *     // property (on constructor, TypeScript only)
      *     class Example {
      *         @Reflect.metadata(key, value)
      *         static staticProperty;
      *     }
      *
      *     // property (on prototype, TypeScript only)
      *     class Example {
      *         @Reflect.metadata(key, value)
      *         property;
      *     }
      *
      *     // method (on constructor)
      *     class Example {
      *         @Reflect.metadata(key, value)
      *         static staticMethod() { }
      *     }
      *
      *     // method (on prototype)
      *     class Example {
      *         @Reflect.metadata(key, value)
      *         method() { }
      *     }
      *
      */
    function metadata(metadataKey, metadataValue) {
        function decorator(target, propertyKey) {
            if (!IsObject(target))
                throw new TypeError();
            if (!IsUndefined(propertyKey) && !IsPropertyKey(propertyKey))
                throw new TypeError();
            OrdinaryDefineOwnMetadata(metadataKey, metadataValue, target, propertyKey);
        }
        return decorator;
    }
    Reflect.metadata = metadata;
    /**
      * Define a unique metadata entry on the target.
      * @param metadataKey A key used to store and retrieve metadata.
      * @param metadataValue A value that contains attached metadata.
      * @param target The target object on which to define metadata.
      * @param propertyKey (Optional) The property key for the target.
      * @example
      *
      *     class Example {
      *         // property declarations are not part of ES6, though they are valid in TypeScript:
      *         // static staticProperty;
      *         // property;
      *
      *         constructor(p) { }
      *         static staticMethod(p) { }
      *         method(p) { }
      *     }
      *
      *     // constructor
      *     Reflect.defineMetadata("custom:annotation", options, Example);
      *
      *     // property (on constructor)
      *     Reflect.defineMetadata("custom:annotation", options, Example, "staticProperty");
      *
      *     // property (on prototype)
      *     Reflect.defineMetadata("custom:annotation", options, Example.prototype, "property");
      *
      *     // method (on constructor)
      *     Reflect.defineMetadata("custom:annotation", options, Example, "staticMethod");
      *
      *     // method (on prototype)
      *     Reflect.defineMetadata("custom:annotation", options, Example.prototype, "method");
      *
      *     // decorator factory as metadata-producing annotation.
      *     function MyAnnotation(options): Decorator {
      *         return (target, key?) => Reflect.defineMetadata("custom:annotation", options, target, key);
      *     }
      *
      */
    function defineMetadata(metadataKey, metadataValue, target, propertyKey) {
        if (!IsObject(target))
            throw new TypeError();
        if (!IsUndefined(propertyKey))
            propertyKey = ToPropertyKey(propertyKey);
        return OrdinaryDefineOwnMetadata(metadataKey, metadataValue, target, propertyKey);
    }
    Reflect.defineMetadata = defineMetadata;
    /**
      * Gets a value indicating whether the target object or its prototype chain has the provided metadata key defined.
      * @param metadataKey A key used to store and retrieve metadata.
      * @param target The target object on which the metadata is defined.
      * @param propertyKey (Optional) The property key for the target.
      * @returns `true` if the metadata key was defined on the target object or its prototype chain; otherwise, `false`.
      * @example
      *
      *     class Example {
      *         // property declarations are not part of ES6, though they are valid in TypeScript:
      *         // static staticProperty;
      *         // property;
      *
      *         constructor(p) { }
      *         static staticMethod(p) { }
      *         method(p) { }
      *     }
      *
      *     // constructor
      *     result = Reflect.hasMetadata("custom:annotation", Example);
      *
      *     // property (on constructor)
      *     result = Reflect.hasMetadata("custom:annotation", Example, "staticProperty");
      *
      *     // property (on prototype)
      *     result = Reflect.hasMetadata("custom:annotation", Example.prototype, "property");
      *
      *     // method (on constructor)
      *     result = Reflect.hasMetadata("custom:annotation", Example, "staticMethod");
      *
      *     // method (on prototype)
      *     result = Reflect.hasMetadata("custom:annotation", Example.prototype, "method");
      *
      */
    function hasMetadata(metadataKey, target, propertyKey) {
        if (!IsObject(target))
            throw new TypeError();
        if (!IsUndefined(propertyKey))
            propertyKey = ToPropertyKey(propertyKey);
        return OrdinaryHasMetadata(metadataKey, target, propertyKey);
    }
    Reflect.hasMetadata = hasMetadata;
    /**
      * Gets a value indicating whether the target object has the provided metadata key defined.
      * @param metadataKey A key used to store and retrieve metadata.
      * @param target The target object on which the metadata is defined.
      * @param propertyKey (Optional) The property key for the target.
      * @returns `true` if the metadata key was defined on the target object; otherwise, `false`.
      * @example
      *
      *     class Example {
      *         // property declarations are not part of ES6, though they are valid in TypeScript:
      *         // static staticProperty;
      *         // property;
      *
      *         constructor(p) { }
      *         static staticMethod(p) { }
      *         method(p) { }
      *     }
      *
      *     // constructor
      *     result = Reflect.hasOwnMetadata("custom:annotation", Example);
      *
      *     // property (on constructor)
      *     result = Reflect.hasOwnMetadata("custom:annotation", Example, "staticProperty");
      *
      *     // property (on prototype)
      *     result = Reflect.hasOwnMetadata("custom:annotation", Example.prototype, "property");
      *
      *     // method (on constructor)
      *     result = Reflect.hasOwnMetadata("custom:annotation", Example, "staticMethod");
      *
      *     // method (on prototype)
      *     result = Reflect.hasOwnMetadata("custom:annotation", Example.prototype, "method");
      *
      */
    function hasOwnMetadata(metadataKey, target, propertyKey) {
        if (!IsObject(target))
            throw new TypeError();
        if (!IsUndefined(propertyKey))
            propertyKey = ToPropertyKey(propertyKey);
        return OrdinaryHasOwnMetadata(metadataKey, target, propertyKey);
    }
    Reflect.hasOwnMetadata = hasOwnMetadata;
    /**
      * Gets the metadata value for the provided metadata key on the target object or its prototype chain.
      * @param metadataKey A key used to store and retrieve metadata.
      * @param target The target object on which the metadata is defined.
      * @param propertyKey (Optional) The property key for the target.
      * @returns The metadata value for the metadata key if found; otherwise, `undefined`.
      * @example
      *
      *     class Example {
      *         // property declarations are not part of ES6, though they are valid in TypeScript:
      *         // static staticProperty;
      *         // property;
      *
      *         constructor(p) { }
      *         static staticMethod(p) { }
      *         method(p) { }
      *     }
      *
      *     // constructor
      *     result = Reflect.getMetadata("custom:annotation", Example);
      *
      *     // property (on constructor)
      *     result = Reflect.getMetadata("custom:annotation", Example, "staticProperty");
      *
      *     // property (on prototype)
      *     result = Reflect.getMetadata("custom:annotation", Example.prototype, "property");
      *
      *     // method (on constructor)
      *     result = Reflect.getMetadata("custom:annotation", Example, "staticMethod");
      *
      *     // method (on prototype)
      *     result = Reflect.getMetadata("custom:annotation", Example.prototype, "method");
      *
      */
    function getMetadata(metadataKey, target, propertyKey) {
        if (!IsObject(target))
            throw new TypeError();
        if (!IsUndefined(propertyKey))
            propertyKey = ToPropertyKey(propertyKey);
        return OrdinaryGetMetadata(metadataKey, target, propertyKey);
    }
    Reflect.getMetadata = getMetadata;
    /**
      * Gets the metadata value for the provided metadata key on the target object.
      * @param metadataKey A key used to store and retrieve metadata.
      * @param target The target object on which the metadata is defined.
      * @param propertyKey (Optional) The property key for the target.
      * @returns The metadata value for the metadata key if found; otherwise, `undefined`.
      * @example
      *
      *     class Example {
      *         // property declarations are not part of ES6, though they are valid in TypeScript:
      *         // static staticProperty;
      *         // property;
      *
      *         constructor(p) { }
      *         static staticMethod(p) { }
      *         method(p) { }
      *     }
      *
      *     // constructor
      *     result = Reflect.getOwnMetadata("custom:annotation", Example);
      *
      *     // property (on constructor)
      *     result = Reflect.getOwnMetadata("custom:annotation", Example, "staticProperty");
      *
      *     // property (on prototype)
      *     result = Reflect.getOwnMetadata("custom:annotation", Example.prototype, "property");
      *
      *     // method (on constructor)
      *     result = Reflect.getOwnMetadata("custom:annotation", Example, "staticMethod");
      *
      *     // method (on prototype)
      *     result = Reflect.getOwnMetadata("custom:annotation", Example.prototype, "method");
      *
      */
    function getOwnMetadata(metadataKey, target, propertyKey) {
        if (!IsObject(target))
            throw new TypeError();
        if (!IsUndefined(propertyKey))
            propertyKey = ToPropertyKey(propertyKey);
        return OrdinaryGetOwnMetadata(metadataKey, target, propertyKey);
    }
    Reflect.getOwnMetadata = getOwnMetadata;
    /**
      * Gets the metadata keys defined on the target object or its prototype chain.
      * @param target The target object on which the metadata is defined.
      * @param propertyKey (Optional) The property key for the target.
      * @returns An array of unique metadata keys.
      * @example
      *
      *     class Example {
      *         // property declarations are not part of ES6, though they are valid in TypeScript:
      *         // static staticProperty;
      *         // property;
      *
      *         constructor(p) { }
      *         static staticMethod(p) { }
      *         method(p) { }
      *     }
      *
      *     // constructor
      *     result = Reflect.getMetadataKeys(Example);
      *
      *     // property (on constructor)
      *     result = Reflect.getMetadataKeys(Example, "staticProperty");
      *
      *     // property (on prototype)
      *     result = Reflect.getMetadataKeys(Example.prototype, "property");
      *
      *     // method (on constructor)
      *     result = Reflect.getMetadataKeys(Example, "staticMethod");
      *
      *     // method (on prototype)
      *     result = Reflect.getMetadataKeys(Example.prototype, "method");
      *
      */
    function getMetadataKeys(target, propertyKey) {
        if (!IsObject(target))
            throw new TypeError();
        if (!IsUndefined(propertyKey))
            propertyKey = ToPropertyKey(propertyKey);
        return OrdinaryMetadataKeys(target, propertyKey);
    }
    Reflect.getMetadataKeys = getMetadataKeys;
    /**
      * Gets the unique metadata keys defined on the target object.
      * @param target The target object on which the metadata is defined.
      * @param propertyKey (Optional) The property key for the target.
      * @returns An array of unique metadata keys.
      * @example
      *
      *     class Example {
      *         // property declarations are not part of ES6, though they are valid in TypeScript:
      *         // static staticProperty;
      *         // property;
      *
      *         constructor(p) { }
      *         static staticMethod(p) { }
      *         method(p) { }
      *     }
      *
      *     // constructor
      *     result = Reflect.getOwnMetadataKeys(Example);
      *
      *     // property (on constructor)
      *     result = Reflect.getOwnMetadataKeys(Example, "staticProperty");
      *
      *     // property (on prototype)
      *     result = Reflect.getOwnMetadataKeys(Example.prototype, "property");
      *
      *     // method (on constructor)
      *     result = Reflect.getOwnMetadataKeys(Example, "staticMethod");
      *
      *     // method (on prototype)
      *     result = Reflect.getOwnMetadataKeys(Example.prototype, "method");
      *
      */
    function getOwnMetadataKeys(target, propertyKey) {
        if (!IsObject(target))
            throw new TypeError();
        if (!IsUndefined(propertyKey))
            propertyKey = ToPropertyKey(propertyKey);
        return OrdinaryOwnMetadataKeys(target, propertyKey);
    }
    Reflect.getOwnMetadataKeys = getOwnMetadataKeys;
    /**
      * Deletes the metadata entry from the target object with the provided key.
      * @param metadataKey A key used to store and retrieve metadata.
      * @param target The target object on which the metadata is defined.
      * @param propertyKey (Optional) The property key for the target.
      * @returns `true` if the metadata entry was found and deleted; otherwise, false.
      * @example
      *
      *     class Example {
      *         // property declarations are not part of ES6, though they are valid in TypeScript:
      *         // static staticProperty;
      *         // property;
      *
      *         constructor(p) { }
      *         static staticMethod(p) { }
      *         method(p) { }
      *     }
      *
      *     // constructor
      *     result = Reflect.deleteMetadata("custom:annotation", Example);
      *
      *     // property (on constructor)
      *     result = Reflect.deleteMetadata("custom:annotation", Example, "staticProperty");
      *
      *     // property (on prototype)
      *     result = Reflect.deleteMetadata("custom:annotation", Example.prototype, "property");
      *
      *     // method (on constructor)
      *     result = Reflect.deleteMetadata("custom:annotation", Example, "staticMethod");
      *
      *     // method (on prototype)
      *     result = Reflect.deleteMetadata("custom:annotation", Example.prototype, "method");
      *
      */
    function deleteMetadata(metadataKey, target, propertyKey) {
        if (!IsObject(target))
            throw new TypeError();
        if (!IsUndefined(propertyKey))
            propertyKey = ToPropertyKey(propertyKey);
        var metadataMap = GetOrCreateMetadataMap(target, propertyKey, /*Create*/ false);
        if (IsUndefined(metadataMap))
            return false;
        if (!metadataMap.delete(metadataKey))
            return false;
        if (metadataMap.size > 0)
            return true;
        var targetMetadata = Metadata.get(target);
        targetMetadata.delete(propertyKey);
        if (targetMetadata.size > 0)
            return true;
        Metadata.delete(target);
        return true;
    }
    Reflect.deleteMetadata = deleteMetadata;
    function DecorateConstructor(decorators, target) {
        for (var i = decorators.length - 1; i >= 0; --i) {
            var decorator = decorators[i];
            var decorated = decorator(target);
            if (!IsUndefined(decorated) && !IsNull(decorated)) {
                if (!IsConstructor(decorated))
                    throw new TypeError();
                target = decorated;
            }
        }
        return target;
    }
    function DecorateProperty(decorators, target, propertyKey, descriptor) {
        for (var i = decorators.length - 1; i >= 0; --i) {
            var decorator = decorators[i];
            var decorated = decorator(target, propertyKey, descriptor);
            if (!IsUndefined(decorated) && !IsNull(decorated)) {
                if (!IsObject(decorated))
                    throw new TypeError();
                descriptor = decorated;
            }
        }
        return descriptor;
    }
    function GetOrCreateMetadataMap(O, P, Create) {
        var targetMetadata = Metadata.get(O);
        if (IsUndefined(targetMetadata)) {
            if (!Create)
                return undefined;
            targetMetadata = new _Map();
            Metadata.set(O, targetMetadata);
        }
        var metadataMap = targetMetadata.get(P);
        if (IsUndefined(metadataMap)) {
            if (!Create)
                return undefined;
            metadataMap = new _Map();
            targetMetadata.set(P, metadataMap);
        }
        return metadataMap;
    }
    // 3.1.1.1 OrdinaryHasMetadata(MetadataKey, O, P)
    // https://rbuckton.github.io/reflect-metadata/#ordinaryhasmetadata
    function OrdinaryHasMetadata(MetadataKey, O, P) {
        var hasOwn = OrdinaryHasOwnMetadata(MetadataKey, O, P);
        if (hasOwn)
            return true;
        var parent = OrdinaryGetPrototypeOf(O);
        if (!IsNull(parent))
            return OrdinaryHasMetadata(MetadataKey, parent, P);
        return false;
    }
    // 3.1.2.1 OrdinaryHasOwnMetadata(MetadataKey, O, P)
    // https://rbuckton.github.io/reflect-metadata/#ordinaryhasownmetadata
    function OrdinaryHasOwnMetadata(MetadataKey, O, P) {
        var metadataMap = GetOrCreateMetadataMap(O, P, /*Create*/ false);
        if (IsUndefined(metadataMap))
            return false;
        return ToBoolean(metadataMap.has(MetadataKey));
    }
    // 3.1.3.1 OrdinaryGetMetadata(MetadataKey, O, P)
    // https://rbuckton.github.io/reflect-metadata/#ordinarygetmetadata
    function OrdinaryGetMetadata(MetadataKey, O, P) {
        var hasOwn = OrdinaryHasOwnMetadata(MetadataKey, O, P);
        if (hasOwn)
            return OrdinaryGetOwnMetadata(MetadataKey, O, P);
        var parent = OrdinaryGetPrototypeOf(O);
        if (!IsNull(parent))
            return OrdinaryGetMetadata(MetadataKey, parent, P);
        return undefined;
    }
    // 3.1.4.1 OrdinaryGetOwnMetadata(MetadataKey, O, P)
    // https://rbuckton.github.io/reflect-metadata/#ordinarygetownmetadata
    function OrdinaryGetOwnMetadata(MetadataKey, O, P) {
        var metadataMap = GetOrCreateMetadataMap(O, P, /*Create*/ false);
        if (IsUndefined(metadataMap))
            return undefined;
        return metadataMap.get(MetadataKey);
    }
    // 3.1.5.1 OrdinaryDefineOwnMetadata(MetadataKey, MetadataValue, O, P)
    // https://rbuckton.github.io/reflect-metadata/#ordinarydefineownmetadata
    function OrdinaryDefineOwnMetadata(MetadataKey, MetadataValue, O, P) {
        var metadataMap = GetOrCreateMetadataMap(O, P, /*Create*/ true);
        metadataMap.set(MetadataKey, MetadataValue);
    }
    // 3.1.6.1 OrdinaryMetadataKeys(O, P)
    // https://rbuckton.github.io/reflect-metadata/#ordinarymetadatakeys
    function OrdinaryMetadataKeys(O, P) {
        var ownKeys = OrdinaryOwnMetadataKeys(O, P);
        var parent = OrdinaryGetPrototypeOf(O);
        if (parent === null)
            return ownKeys;
        var parentKeys = OrdinaryMetadataKeys(parent, P);
        if (parentKeys.length <= 0)
            return ownKeys;
        if (ownKeys.length <= 0)
            return parentKeys;
        var set = new _Set();
        var keys = [];
        for (var _i = 0, ownKeys_1 = ownKeys; _i < ownKeys_1.length; _i++) {
            var key = ownKeys_1[_i];
            var hasKey = set.has(key);
            if (!hasKey) {
                set.add(key);
                keys.push(key);
            }
        }
        for (var _a = 0, parentKeys_1 = parentKeys; _a < parentKeys_1.length; _a++) {
            var key = parentKeys_1[_a];
            var hasKey = set.has(key);
            if (!hasKey) {
                set.add(key);
                keys.push(key);
            }
        }
        return keys;
    }
    // 3.1.7.1 OrdinaryOwnMetadataKeys(O, P)
    // https://rbuckton.github.io/reflect-metadata/#ordinaryownmetadatakeys
    function OrdinaryOwnMetadataKeys(O, P) {
        var keys = [];
        var metadataMap = GetOrCreateMetadataMap(O, P, /*Create*/ false);
        if (IsUndefined(metadataMap))
            return keys;
        var keysObj = metadataMap.keys();
        var iterator = GetIterator(keysObj);
        var k = 0;
        while (true) {
            var next = IteratorStep(iterator);
            if (!next) {
                keys.length = k;
                return keys;
            }
            var nextValue = IteratorValue(next);
            try {
                keys[k] = nextValue;
            }
            catch (e) {
                try {
                    IteratorClose(iterator);
                }
                finally {
                    throw e;
                }
            }
            k++;
        }
    }
    // 6 ECMAScript Data Typ0es and Values
    // https://tc39.github.io/ecma262/#sec-ecmascript-data-types-and-values
    function Type(x) {
        if (x === null)
            return 1 /* Null */;
        switch (typeof x) {
            case "undefined": return 0 /* Undefined */;
            case "boolean": return 2 /* Boolean */;
            case "string": return 3 /* String */;
            case "symbol": return 4 /* Symbol */;
            case "number": return 5 /* Number */;
            case "object": return x === null ? 1 /* Null */ : 6 /* Object */;
            default: return 6 /* Object */;
        }
    }
    // 6.1.1 The Undefined Type
    // https://tc39.github.io/ecma262/#sec-ecmascript-language-types-undefined-type
    function IsUndefined(x) {
        return x === undefined;
    }
    // 6.1.2 The Null Type
    // https://tc39.github.io/ecma262/#sec-ecmascript-language-types-null-type
    function IsNull(x) {
        return x === null;
    }
    // 6.1.5 The Symbol Type
    // https://tc39.github.io/ecma262/#sec-ecmascript-language-types-symbol-type
    function IsSymbol(x) {
        return typeof x === "symbol";
    }
    // 6.1.7 The Object Type
    // https://tc39.github.io/ecma262/#sec-object-type
    function IsObject(x) {
        return typeof x === "object" ? x !== null : typeof x === "function";
    }
    // 7.1 Type Conversion
    // https://tc39.github.io/ecma262/#sec-type-conversion
    // 7.1.1 ToPrimitive(input [, PreferredType])
    // https://tc39.github.io/ecma262/#sec-toprimitive
    function ToPrimitive(input, PreferredType) {
        switch (Type(input)) {
            case 0 /* Undefined */: return input;
            case 1 /* Null */: return input;
            case 2 /* Boolean */: return input;
            case 3 /* String */: return input;
            case 4 /* Symbol */: return input;
            case 5 /* Number */: return input;
        }
        var hint = PreferredType === 3 /* String */ ? "string" : PreferredType === 5 /* Number */ ? "number" : "default";
        var exoticToPrim = GetMethod(input, toPrimitiveSymbol);
        if (exoticToPrim !== undefined) {
            var result = exoticToPrim.call(input, hint);
            if (IsObject(result))
                throw new TypeError();
            return result;
        }
        return OrdinaryToPrimitive(input, hint === "default" ? "number" : hint);
    }
    // 7.1.1.1 OrdinaryToPrimitive(O, hint)
    // https://tc39.github.io/ecma262/#sec-ordinarytoprimitive
    function OrdinaryToPrimitive(O, hint) {
        if (hint === "string") {
            var toString_1 = O.toString;
            if (IsCallable(toString_1)) {
                var result = toString_1.call(O);
                if (!IsObject(result))
                    return result;
            }
            var valueOf = O.valueOf;
            if (IsCallable(valueOf)) {
                var result = valueOf.call(O);
                if (!IsObject(result))
                    return result;
            }
        }
        else {
            var valueOf = O.valueOf;
            if (IsCallable(valueOf)) {
                var result = valueOf.call(O);
                if (!IsObject(result))
                    return result;
            }
            var toString_2 = O.toString;
            if (IsCallable(toString_2)) {
                var result = toString_2.call(O);
                if (!IsObject(result))
                    return result;
            }
        }
        throw new TypeError();
    }
    // 7.1.2 ToBoolean(argument)
    // https://tc39.github.io/ecma262/2016/#sec-toboolean
    function ToBoolean(argument) {
        return !!argument;
    }
    // 7.1.12 ToString(argument)
    // https://tc39.github.io/ecma262/#sec-tostring
    function ToString(argument) {
        return "" + argument;
    }
    // 7.1.14 ToPropertyKey(argument)
    // https://tc39.github.io/ecma262/#sec-topropertykey
    function ToPropertyKey(argument) {
        var key = ToPrimitive(argument, 3 /* String */);
        if (IsSymbol(key))
            return key;
        return ToString(key);
    }
    // 7.2 Testing and Comparison Operations
    // https://tc39.github.io/ecma262/#sec-testing-and-comparison-operations
    // 7.2.2 IsArray(argument)
    // https://tc39.github.io/ecma262/#sec-isarray
    function IsArray(argument) {
        return Array.isArray
            ? Array.isArray(argument)
            : argument instanceof Object
                ? argument instanceof Array
                : Object.prototype.toString.call(argument) === "[object Array]";
    }
    // 7.2.3 IsCallable(argument)
    // https://tc39.github.io/ecma262/#sec-iscallable
    function IsCallable(argument) {
        // NOTE: This is an approximation as we cannot check for [[Call]] internal method.
        return typeof argument === "function";
    }
    // 7.2.4 IsConstructor(argument)
    // https://tc39.github.io/ecma262/#sec-isconstructor
    function IsConstructor(argument) {
        // NOTE: This is an approximation as we cannot check for [[Construct]] internal method.
        return typeof argument === "function";
    }
    // 7.2.7 IsPropertyKey(argument)
    // https://tc39.github.io/ecma262/#sec-ispropertykey
    function IsPropertyKey(argument) {
        switch (Type(argument)) {
            case 3 /* String */: return true;
            case 4 /* Symbol */: return true;
            default: return false;
        }
    }
    // 7.3 Operations on Objects
    // https://tc39.github.io/ecma262/#sec-operations-on-objects
    // 7.3.9 GetMethod(V, P)
    // https://tc39.github.io/ecma262/#sec-getmethod
    function GetMethod(V, P) {
        var func = V[P];
        if (func === undefined || func === null)
            return undefined;
        if (!IsCallable(func))
            throw new TypeError();
        return func;
    }
    // 7.4 Operations on Iterator Objects
    // https://tc39.github.io/ecma262/#sec-operations-on-iterator-objects
    function GetIterator(obj) {
        var method = GetMethod(obj, iteratorSymbol);
        if (!IsCallable(method))
            throw new TypeError(); // from Call
        var iterator = method.call(obj);
        if (!IsObject(iterator))
            throw new TypeError();
        return iterator;
    }
    // 7.4.4 IteratorValue(iterResult)
    // https://tc39.github.io/ecma262/2016/#sec-iteratorvalue
    function IteratorValue(iterResult) {
        return iterResult.value;
    }
    // 7.4.5 IteratorStep(iterator)
    // https://tc39.github.io/ecma262/#sec-iteratorstep
    function IteratorStep(iterator) {
        var result = iterator.next();
        return result.done ? false : result;
    }
    // 7.4.6 IteratorClose(iterator, completion)
    // https://tc39.github.io/ecma262/#sec-iteratorclose
    function IteratorClose(iterator) {
        var f = iterator["return"];
        if (f)
            f.call(iterator);
    }
    // 9.1 Ordinary Object Internal Methods and Internal Slots
    // https://tc39.github.io/ecma262/#sec-ordinary-object-internal-methods-and-internal-slots
    // 9.1.1.1 OrdinaryGetPrototypeOf(O)
    // https://tc39.github.io/ecma262/#sec-ordinarygetprototypeof
    function OrdinaryGetPrototypeOf(O) {
        var proto = Object.getPrototypeOf(O);
        if (typeof O !== "function" || O === functionPrototype)
            return proto;
        // TypeScript doesn't set __proto__ in ES5, as it's non-standard.
        // Try to determine the superclass constructor. Compatible implementations
        // must either set __proto__ on a subclass constructor to the superclass constructor,
        // or ensure each class has a valid `constructor` property on its prototype that
        // points back to the constructor.
        // If this is not the same as Function.[[Prototype]], then this is definately inherited.
        // This is the case when in ES6 or when using __proto__ in a compatible browser.
        if (proto !== functionPrototype)
            return proto;
        // If the super prototype is Object.prototype, null, or undefined, then we cannot determine the heritage.
        var prototype = O.prototype;
        var prototypeProto = prototype && Object.getPrototypeOf(prototype);
        if (prototypeProto == null || prototypeProto === Object.prototype)
            return proto;
        // If the constructor was not a function, then we cannot determine the heritage.
        var constructor = prototypeProto.constructor;
        if (typeof constructor !== "function")
            return proto;
        // If we have some kind of self-reference, then we cannot determine the heritage.
        if (constructor === O)
            return proto;
        // we have a pretty good guess at the heritage.
        return constructor;
    }
    // naive Map shim
    function CreateMapPolyfill() {
        var cacheSentinel = {};
        var arraySentinel = [];
        var MapIterator = (function () {
            function MapIterator(keys, values, selector) {
                this._index = 0;
                this._keys = keys;
                this._values = values;
                this._selector = selector;
            }
            MapIterator.prototype["@@iterator"] = function () { return this; };
            MapIterator.prototype[iteratorSymbol] = function () { return this; };
            MapIterator.prototype.next = function () {
                var index = this._index;
                if (index >= 0 && index < this._keys.length) {
                    var result = this._selector(this._keys[index], this._values[index]);
                    if (index + 1 >= this._keys.length) {
                        this._index = -1;
                        this._keys = arraySentinel;
                        this._values = arraySentinel;
                    }
                    else {
                        this._index++;
                    }
                    return { value: result, done: false };
                }
                return { value: undefined, done: true };
            };
            MapIterator.prototype.throw = function (error) {
                if (this._index >= 0) {
                    this._index = -1;
                    this._keys = arraySentinel;
                    this._values = arraySentinel;
                }
                throw error;
            };
            MapIterator.prototype.return = function (value) {
                if (this._index >= 0) {
                    this._index = -1;
                    this._keys = arraySentinel;
                    this._values = arraySentinel;
                }
                return { value: value, done: true };
            };
            return MapIterator;
        }());
        return (function () {
            function Map() {
                this._keys = [];
                this._values = [];
                this._cacheKey = cacheSentinel;
                this._cacheIndex = -2;
            }
            Object.defineProperty(Map.prototype, "size", {
                get: function () { return this._keys.length; },
                enumerable: true,
                configurable: true
            });
            Map.prototype.has = function (key) { return this._find(key, /*insert*/ false) >= 0; };
            Map.prototype.get = function (key) {
                var index = this._find(key, /*insert*/ false);
                return index >= 0 ? this._values[index] : undefined;
            };
            Map.prototype.set = function (key, value) {
                var index = this._find(key, /*insert*/ true);
                this._values[index] = value;
                return this;
            };
            Map.prototype.delete = function (key) {
                var index = this._find(key, /*insert*/ false);
                if (index >= 0) {
                    var size = this._keys.length;
                    for (var i = index + 1; i < size; i++) {
                        this._keys[i - 1] = this._keys[i];
                        this._values[i - 1] = this._values[i];
                    }
                    this._keys.length--;
                    this._values.length--;
                    if (key === this._cacheKey) {
                        this._cacheKey = cacheSentinel;
                        this._cacheIndex = -2;
                    }
                    return true;
                }
                return false;
            };
            Map.prototype.clear = function () {
                this._keys.length = 0;
                this._values.length = 0;
                this._cacheKey = cacheSentinel;
                this._cacheIndex = -2;
            };
            Map.prototype.keys = function () { return new MapIterator(this._keys, this._values, getKey); };
            Map.prototype.values = function () { return new MapIterator(this._keys, this._values, getValue); };
            Map.prototype.entries = function () { return new MapIterator(this._keys, this._values, getEntry); };
            Map.prototype["@@iterator"] = function () { return this.entries(); };
            Map.prototype[iteratorSymbol] = function () { return this.entries(); };
            Map.prototype._find = function (key, insert) {
                if (this._cacheKey !== key) {
                    this._cacheIndex = this._keys.indexOf(this._cacheKey = key);
                }
                if (this._cacheIndex < 0 && insert) {
                    this._cacheIndex = this._keys.length;
                    this._keys.push(key);
                    this._values.push(undefined);
                }
                return this._cacheIndex;
            };
            return Map;
        }());
        function getKey(key, _) {
            return key;
        }
        function getValue(_, value) {
            return value;
        }
        function getEntry(key, value) {
            return [key, value];
        }
    }
    // naive Set shim
    function CreateSetPolyfill() {
        return (function () {
            function Set() {
                this._map = new _Map();
            }
            Object.defineProperty(Set.prototype, "size", {
                get: function () { return this._map.size; },
                enumerable: true,
                configurable: true
            });
            Set.prototype.has = function (value) { return this._map.has(value); };
            Set.prototype.add = function (value) { return this._map.set(value, value), this; };
            Set.prototype.delete = function (value) { return this._map.delete(value); };
            Set.prototype.clear = function () { this._map.clear(); };
            Set.prototype.keys = function () { return this._map.keys(); };
            Set.prototype.values = function () { return this._map.values(); };
            Set.prototype.entries = function () { return this._map.entries(); };
            Set.prototype["@@iterator"] = function () { return this.keys(); };
            Set.prototype[iteratorSymbol] = function () { return this.keys(); };
            return Set;
        }());
    }
    // naive WeakMap shim
    function CreateWeakMapPolyfill() {
        var UUID_SIZE = 16;
        var keys = HashMap.create();
        var rootKey = CreateUniqueKey();
        return (function () {
            function WeakMap() {
                this._key = CreateUniqueKey();
            }
            WeakMap.prototype.has = function (target) {
                var table = GetOrCreateWeakMapTable(target, /*create*/ false);
                return table !== undefined ? HashMap.has(table, this._key) : false;
            };
            WeakMap.prototype.get = function (target) {
                var table = GetOrCreateWeakMapTable(target, /*create*/ false);
                return table !== undefined ? HashMap.get(table, this._key) : undefined;
            };
            WeakMap.prototype.set = function (target, value) {
                var table = GetOrCreateWeakMapTable(target, /*create*/ true);
                table[this._key] = value;
                return this;
            };
            WeakMap.prototype.delete = function (target) {
                var table = GetOrCreateWeakMapTable(target, /*create*/ false);
                return table !== undefined ? delete table[this._key] : false;
            };
            WeakMap.prototype.clear = function () {
                // NOTE: not a real clear, just makes the previous data unreachable
                this._key = CreateUniqueKey();
            };
            return WeakMap;
        }());
        function CreateUniqueKey() {
            var key;
            do
                key = "@@WeakMap@@" + CreateUUID();
            while (HashMap.has(keys, key));
            keys[key] = true;
            return key;
        }
        function GetOrCreateWeakMapTable(target, create) {
            if (!hasOwn.call(target, rootKey)) {
                if (!create)
                    return undefined;
                Object.defineProperty(target, rootKey, { value: HashMap.create() });
            }
            return target[rootKey];
        }
        function FillRandomBytes(buffer, size) {
            for (var i = 0; i < size; ++i)
                buffer[i] = Math.random() * 0xff | 0;
            return buffer;
        }
        function GenRandomBytes(size) {
            if (typeof Uint8Array === "function") {
                if (typeof crypto !== "undefined")
                    return crypto.getRandomValues(new Uint8Array(size));
                if (typeof msCrypto !== "undefined")
                    return msCrypto.getRandomValues(new Uint8Array(size));
                return FillRandomBytes(new Uint8Array(size), size);
            }
            return FillRandomBytes(new Array(size), size);
        }
        function CreateUUID() {
            var data = GenRandomBytes(UUID_SIZE);
            // mark as random - RFC 4122 § 4.4
            data[6] = data[6] & 0x4f | 0x40;
            data[8] = data[8] & 0xbf | 0x80;
            var result = "";
            for (var offset = 0; offset < UUID_SIZE; ++offset) {
                var byte = data[offset];
                if (offset === 4 || offset === 6 || offset === 8)
                    result += "-";
                if (byte < 16)
                    result += "0";
                result += byte.toString(16).toLowerCase();
            }
            return result;
        }
    }
    // uses a heuristic used by v8 and chakra to force an object into dictionary mode.
    function MakeDictionary(obj) {
        obj.__ = undefined;
        delete obj.__;
        return obj;
    }
    // patch global Reflect
    (function (__global) {
        if (typeof __global.Reflect !== "undefined") {
            if (__global.Reflect !== Reflect) {
                for (var p in Reflect) {
                    if (hasOwn.call(Reflect, p)) {
                        __global.Reflect[p] = Reflect[p];
                    }
                }
            }
        }
        else {
            __global.Reflect = Reflect;
        }
    })(typeof global !== "undefined" ? global :
        typeof self !== "undefined" ? self :
            Function("return this;")());
})(Reflect || (Reflect = {}));
//# sourceMappingURL=Reflect.js.map
/* WEBPACK VAR INJECTION */}.call(exports, __webpack_require__(27), __webpack_require__(31)))

/***/ }),
/* 7 */
/***/ (function(module, exports, __webpack_require__) {

module.exports = (__webpack_require__(0))(40);

/***/ }),
/* 8 */
/***/ (function(module, exports, __webpack_require__) {

module.exports = (__webpack_require__(0))(42);

/***/ }),
/* 9 */
/***/ (function(module, exports, __webpack_require__) {

module.exports = (__webpack_require__(0))(47);

/***/ }),
/* 10 */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "a", function() { return AppModuleShared; });
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_0__angular_core__ = __webpack_require__(1);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_1__angular_common__ = __webpack_require__(32);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_2__angular_forms__ = __webpack_require__(28);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_3__angular_http__ = __webpack_require__(4);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_4__angular_router__ = __webpack_require__(29);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_5__components_app_app_component__ = __webpack_require__(3);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_6__components_navmenu_navmenu_component__ = __webpack_require__(14);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_7__components_home_home_component__ = __webpack_require__(13);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_8__components_fetchdata_fetchdata_component__ = __webpack_require__(12);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_9__components_counter_counter_component__ = __webpack_require__(11);
var __decorate = (this && this.__decorate) || function (decorators, target, key, desc) {
    var c = arguments.length, r = c < 3 ? target : desc === null ? desc = Object.getOwnPropertyDescriptor(target, key) : desc, d;
    if (typeof Reflect === "object" && typeof Reflect.decorate === "function") r = Reflect.decorate(decorators, target, key, desc);
    else for (var i = decorators.length - 1; i >= 0; i--) if (d = decorators[i]) r = (c < 3 ? d(r) : c > 3 ? d(target, key, r) : d(target, key)) || r;
    return c > 3 && r && Object.defineProperty(target, key, r), r;
};










var AppModuleShared = (function () {
    function AppModuleShared() {
    }
    AppModuleShared = __decorate([
        __webpack_require__.i(__WEBPACK_IMPORTED_MODULE_0__angular_core__["NgModule"])({
            declarations: [
                __WEBPACK_IMPORTED_MODULE_5__components_app_app_component__["a" /* AppComponent */],
                __WEBPACK_IMPORTED_MODULE_6__components_navmenu_navmenu_component__["a" /* NavMenuComponent */],
                __WEBPACK_IMPORTED_MODULE_9__components_counter_counter_component__["a" /* CounterComponent */],
                __WEBPACK_IMPORTED_MODULE_8__components_fetchdata_fetchdata_component__["a" /* FetchDataComponent */],
                __WEBPACK_IMPORTED_MODULE_7__components_home_home_component__["a" /* HomeComponent */]
            ],
            imports: [
                __WEBPACK_IMPORTED_MODULE_1__angular_common__["CommonModule"],
                __WEBPACK_IMPORTED_MODULE_3__angular_http__["HttpModule"],
                __WEBPACK_IMPORTED_MODULE_2__angular_forms__["FormsModule"],
                __WEBPACK_IMPORTED_MODULE_4__angular_router__["RouterModule"].forRoot([
                    { path: '', redirectTo: 'home', pathMatch: 'full' },
                    { path: 'home', component: __WEBPACK_IMPORTED_MODULE_7__components_home_home_component__["a" /* HomeComponent */] },
                    { path: 'counter', component: __WEBPACK_IMPORTED_MODULE_9__components_counter_counter_component__["a" /* CounterComponent */] },
                    { path: 'fetch-data', component: __WEBPACK_IMPORTED_MODULE_8__components_fetchdata_fetchdata_component__["a" /* FetchDataComponent */] },
                    { path: '**', redirectTo: 'home' }
                ])
            ]
        })
    ], AppModuleShared);
    return AppModuleShared;
}());



/***/ }),
/* 11 */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "a", function() { return CounterComponent; });
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_0__angular_core__ = __webpack_require__(1);
var __decorate = (this && this.__decorate) || function (decorators, target, key, desc) {
    var c = arguments.length, r = c < 3 ? target : desc === null ? desc = Object.getOwnPropertyDescriptor(target, key) : desc, d;
    if (typeof Reflect === "object" && typeof Reflect.decorate === "function") r = Reflect.decorate(decorators, target, key, desc);
    else for (var i = decorators.length - 1; i >= 0; i--) if (d = decorators[i]) r = (c < 3 ? d(r) : c > 3 ? d(target, key, r) : d(target, key)) || r;
    return c > 3 && r && Object.defineProperty(target, key, r), r;
};

var CounterComponent = (function () {
    function CounterComponent() {
        this.currentCount = 0;
    }
    CounterComponent.prototype.incrementCounter = function () {
        this.currentCount++;
    };
    CounterComponent = __decorate([
        __webpack_require__.i(__WEBPACK_IMPORTED_MODULE_0__angular_core__["Component"])({
            selector: 'counter',
            template: __webpack_require__(20)
        })
    ], CounterComponent);
    return CounterComponent;
}());



/***/ }),
/* 12 */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "a", function() { return FetchDataComponent; });
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_0__angular_core__ = __webpack_require__(1);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_1__angular_http__ = __webpack_require__(4);
var __decorate = (this && this.__decorate) || function (decorators, target, key, desc) {
    var c = arguments.length, r = c < 3 ? target : desc === null ? desc = Object.getOwnPropertyDescriptor(target, key) : desc, d;
    if (typeof Reflect === "object" && typeof Reflect.decorate === "function") r = Reflect.decorate(decorators, target, key, desc);
    else for (var i = decorators.length - 1; i >= 0; i--) if (d = decorators[i]) r = (c < 3 ? d(r) : c > 3 ? d(target, key, r) : d(target, key)) || r;
    return c > 3 && r && Object.defineProperty(target, key, r), r;
};
var __metadata = (this && this.__metadata) || function (k, v) {
    if (typeof Reflect === "object" && typeof Reflect.metadata === "function") return Reflect.metadata(k, v);
};
var __param = (this && this.__param) || function (paramIndex, decorator) {
    return function (target, key) { decorator(target, key, paramIndex); }
};


var FetchDataComponent = (function () {
    function FetchDataComponent(http, baseUrl) {
        var _this = this;
        //FormApp.openById('1234567890abcdefghijklmnopqrstuvwxyz');
        http.get(baseUrl + 'api/SampleData/WeatherForecasts').subscribe(function (result) {
            _this.forecasts = result.json();
        }, function (error) { return console.error(error); });
    }
    FetchDataComponent = __decorate([
        __webpack_require__.i(__WEBPACK_IMPORTED_MODULE_0__angular_core__["Component"])({
            selector: 'fetchdata',
            template: __webpack_require__(21),
            styles: [__webpack_require__(25)]
        }),
        __param(1, __webpack_require__.i(__WEBPACK_IMPORTED_MODULE_0__angular_core__["Inject"])('BASE_URL')),
        __metadata("design:paramtypes", [__WEBPACK_IMPORTED_MODULE_1__angular_http__["Http"], String])
    ], FetchDataComponent);
    return FetchDataComponent;
}());



/***/ }),
/* 13 */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "a", function() { return HomeComponent; });
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_0__angular_core__ = __webpack_require__(1);
var __decorate = (this && this.__decorate) || function (decorators, target, key, desc) {
    var c = arguments.length, r = c < 3 ? target : desc === null ? desc = Object.getOwnPropertyDescriptor(target, key) : desc, d;
    if (typeof Reflect === "object" && typeof Reflect.decorate === "function") r = Reflect.decorate(decorators, target, key, desc);
    else for (var i = decorators.length - 1; i >= 0; i--) if (d = decorators[i]) r = (c < 3 ? d(r) : c > 3 ? d(target, key, r) : d(target, key)) || r;
    return c > 3 && r && Object.defineProperty(target, key, r), r;
};

var HomeComponent = (function () {
    function HomeComponent() {
    }
    HomeComponent = __decorate([
        __webpack_require__.i(__WEBPACK_IMPORTED_MODULE_0__angular_core__["Component"])({
            selector: 'home',
            template: __webpack_require__(22)
        })
    ], HomeComponent);
    return HomeComponent;
}());



/***/ }),
/* 14 */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "a", function() { return NavMenuComponent; });
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_0__angular_core__ = __webpack_require__(1);
var __decorate = (this && this.__decorate) || function (decorators, target, key, desc) {
    var c = arguments.length, r = c < 3 ? target : desc === null ? desc = Object.getOwnPropertyDescriptor(target, key) : desc, d;
    if (typeof Reflect === "object" && typeof Reflect.decorate === "function") r = Reflect.decorate(decorators, target, key, desc);
    else for (var i = decorators.length - 1; i >= 0; i--) if (d = decorators[i]) r = (c < 3 ? d(r) : c > 3 ? d(target, key, r) : d(target, key)) || r;
    return c > 3 && r && Object.defineProperty(target, key, r), r;
};

var NavMenuComponent = (function () {
    function NavMenuComponent() {
    }
    NavMenuComponent = __decorate([
        __webpack_require__.i(__WEBPACK_IMPORTED_MODULE_0__angular_core__["Component"])({
            selector: 'nav-menu',
            template: __webpack_require__(23),
            styles: [__webpack_require__(26)]
        })
    ], NavMenuComponent);
    return NavMenuComponent;
}());



/***/ }),
/* 15 */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
Object.defineProperty(__webpack_exports__, "__esModule", { value: true });
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_0_reflect_metadata__ = __webpack_require__(6);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_0_reflect_metadata___default = __webpack_require__.n(__WEBPACK_IMPORTED_MODULE_0_reflect_metadata__);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_1_zone_js__ = __webpack_require__(9);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_1_zone_js___default = __webpack_require__.n(__WEBPACK_IMPORTED_MODULE_1_zone_js__);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_2_bootstrap__ = __webpack_require__(8);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_2_bootstrap___default = __webpack_require__.n(__WEBPACK_IMPORTED_MODULE_2_bootstrap__);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_3__angular_core__ = __webpack_require__(1);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_4__angular_platform_browser_dynamic__ = __webpack_require__(7);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_5__app_app_module_browser__ = __webpack_require__(5);






if (false) {
    module.hot.accept();
    module.hot.dispose(function () {
        // Before restarting the app, we create a new root element and dispose the old one
        var oldRootElem = document.querySelector('app');
        var newRootElem = document.createElement('app');
        oldRootElem.parentNode.insertBefore(newRootElem, oldRootElem);
        modulePromise.then(function (appModule) { return appModule.destroy(); });
    });
}
else {
    __webpack_require__.i(__WEBPACK_IMPORTED_MODULE_3__angular_core__["enableProdMode"])();
}
// Note: @ng-tools/webpack looks for the following expression when performing production
// builds. Don't change how this line looks, otherwise you may break tree-shaking.
var modulePromise = __webpack_require__.i(__WEBPACK_IMPORTED_MODULE_4__angular_platform_browser_dynamic__["platformBrowserDynamic"])().bootstrapModule(__WEBPACK_IMPORTED_MODULE_5__app_app_module_browser__["a" /* AppModule */]);


/***/ }),
/* 16 */
/***/ (function(module, exports, __webpack_require__) {

exports = module.exports = __webpack_require__(2)(undefined);
// imports


// module
exports.push([module.i, "@media (max-width: 767px) {\r\n    /* On small screens, the nav menu spans the full width of the screen. Leave a space for it. */\r\n    .body-content {\r\n        padding-top: 50px;\r\n    }\r\n}\r\n", ""]);

// exports


/***/ }),
/* 17 */
/***/ (function(module, exports, __webpack_require__) {

exports = module.exports = __webpack_require__(2)(undefined);
// imports


// module
exports.push([module.i, ".freebirdThemedTab .exportTab .freebirdThemedBadge {\r\n    background-color: rgba(0, 0, 0, 0.5);\r\n}\r\n\r\n.freebirdThemedTab .exportTab.isSelected .freebirdThemedBadge {\r\n    background-color: rgb(110, 92, 52);\r\n}\r\n\r\n.freebirdThemedCheckbox.isChecked:not(.isDisabled) {\r\n    border-color: rgb(110, 92, 52);\r\n}\r\n\r\n.freebirdThemedCheckbox.isCheckedNext > .exportInk, .freebirdThemedCheckbox.isFocused > .exportInk {\r\n    background-color: rgba(110, 92, 52, 0.15);\r\n}\r\n\r\n.freebirdThemedCheckboxDarkerDisabled.isChecked.isDisabled {\r\n    border-color: rgb(110, 92, 52);\r\n}\r\n\r\n.freebirdThemedCheckboxDarkerDisabled.isDisabled:not(.isChecked) {\r\n    border-color: rgba(0, 0, 0, .26);\r\n}\r\n\r\n.freebirdQuizResponse .freebirdThemedCheckboxDarkerDisabled.isChecked.isDisabled {\r\n    border-color: rgba(0, 0, 0, .54);\r\n}\r\n\r\n.freebirdThemedRadio.isChecked:not(.isDisabled) .exportOuterCircle, .freebirdThemedRadio .exportInnerCircle {\r\n    border-color: rgb(110, 92, 52);\r\n}\r\n\r\n.freebirdThemedRadio.isCheckedNext > .exportInk, .freebirdThemedRadio.isFocused > .exportInk {\r\n    background-color: rgba(110, 92, 52, 0.15);\r\n}\r\n\r\n.freebirdThemedRadioDarkerDisabled.isChecked.isDisabled .exportOuterCircle, .freebirdThemedRadioDarkerDisabled.isChecked.isDisabled .exportInnerCircle {\r\n    border-color: rgb(110, 92, 52);\r\n}\r\n\r\n.freebirdThemedRadioDarkerDisabled.isDisabled:not(.isChecked) .exportOuterCircle {\r\n    border-color: rgba(0, 0, 0, .26);\r\n}\r\n\r\n.freebirdQuizResponse .freebirdThemedRadioDarkerDisabled.isChecked.isDisabled .exportOuterCircle, .freebirdQuizResponse .freebirdThemedRadioDarkerDisabled.isChecked.isDisabled .exportInnerCircle {\r\n    border-color: rgba(0, 0, 0, .54);\r\n}\r\n\r\n.freebirdThemedSelectOptionDarkerDisabled.isSelected.isDisabled {\r\n    color: rgba(0, 0, 0, .87);\r\n}\r\n\r\n.freebirdThemedFlatButton .exportInk {\r\n    background-image: radial-gradient( circle farthest-side,rgba(110, 92, 52, 0.15),rgba(110, 92, 52, 0.15) 80%,rgba(110, 92, 52, 0) 100% );\r\n}\r\n\r\n.freebirdThemedFlatButton {\r\n    color: rgb(110, 92, 52);\r\n}\r\n\r\n    .freebirdThemedFlatButton.isDisabled {\r\n        color: rgba(110, 92, 52, 0.5);\r\n    }\r\n\r\n    .freebirdThemedFlatButton.isFocused .exportOverlay {\r\n        background-color: rgba(110, 92, 52, 0.15);\r\n    }\r\n\r\n.freebirdThemedFlatButtonInverted .exportInk {\r\n    background-image: radial-gradient( circle farthest-side,rgba(255, 255, 255, .3),rgba(255, 255, 255, .3) 80%,rgba(255, 255, 255, 0) 100% );\r\n}\r\n\r\n.freebirdThemedFlatButtonInverted {\r\n    background-color: rgb(110, 92, 52);\r\n    color: rgba(255, 255, 255, 1);\r\n}\r\n\r\n    .freebirdThemedFlatButtonInverted.isDisabled {\r\n        color: rgba(255, 255, 255, 1);\r\n        opacity: .54;\r\n    }\r\n\r\n    .freebirdThemedFlatButtonInverted.isFocused .exportOverlay {\r\n        background-color: rgba(255, 255, 255, .3);\r\n    }\r\n\r\n.freebirdThemedTab .exportIndicator {\r\n    background-color: rgb(110, 92, 52);\r\n}\r\n\r\n.freebirdThemedTab .exportTab {\r\n    color: rgba(0, 0, 0, 0.5);\r\n}\r\n\r\n    .freebirdThemedTab .exportTab.isSelected {\r\n        color: rgb(110, 92, 52);\r\n    }\r\n\r\n.freebirdThemedTab .exportTabPageButton {\r\n    fill: rgb(110, 92, 52);\r\n}\r\n\r\n.freebirdThemedTab .exportInk {\r\n    background-image: radial-gradient( circle farthest-side,rgba(110, 92, 52, 0.15),rgba(110, 92, 52, 0.15) 80%,rgba(110, 92, 52, 0) 100% );\r\n}\r\n\r\n.freebirdThemedDarkTab {\r\n    background-color: rgb(110, 92, 52);\r\n}\r\n\r\n    .freebirdThemedDarkTab .freebirdThemedDarkTabContent {\r\n        background-color: #FFF;\r\n    }\r\n\r\n    .freebirdThemedDarkTab .exportIndicator {\r\n        background-color: rgba(255, 255, 255, 1);\r\n    }\r\n\r\n    .freebirdThemedDarkTab .exportTab {\r\n        color: rgba(255, 255, 255, 0.5);\r\n    }\r\n\r\n        .freebirdThemedDarkTab .exportTab.isSelected {\r\n            color: rgba(255, 255, 255, 1);\r\n            background-color: rgba(255, 255, 255, 0);\r\n        }\r\n\r\n        .freebirdThemedDarkTab .exportTab.isFocused {\r\n            background-color: rgba(255, 255, 255, 0.15);\r\n        }\r\n\r\n    .freebirdThemedDarkTab .exportTabPageButton {\r\n        fill: rgba(255, 255, 255, 1);\r\n    }\r\n\r\n    .freebirdThemedDarkTab .exportInk {\r\n        background-image: radial-gradient( circle farthest-side,rgba(255, 255, 255, 0.15),rgba(255, 255, 255, 0.15) 80%,rgba(255, 255, 255, 0) 100% );\r\n    }\r\n\r\n.freebirdThemedTextarea .exportFocusUnderline {\r\n    background-color: rgb(110, 92, 52);\r\n}\r\n\r\n.freebirdThemedTextarea .exportHint {\r\n    color: rgb(110, 92, 52);\r\n}\r\n\r\n.freebirdThemedTextarea.exportHasError .exportContent.exportUnderline, .freebirdThemedTextarea.exportHasError .exportContent.exportFocusUnderline {\r\n    background-color: rgb(224, 218, 203);\r\n}\r\n\r\n.quantumWizTextinputPaperinputInput:not([disabled]):focus ~ .quantumWizTextinputPaperinputFloatingLabel.exportLabel, .isFocused > .exportContent > .quantumWizTextinputPapertextareaFloatingLabel.exportLabel, .isFocused.modeDark > .exportContent > .quantumWizTextinputPapertextareaFloatingLabel.exportLabel {\r\n    color: rgb(110, 92, 52);\r\n}\r\n\r\n.freebirdThemedTextareaDarkerDisabled.isDisabled .exportInput[disabled] {\r\n    color: rgba(0, 0, 0, .87);\r\n}\r\n\r\n.freebirdThemedInput .exportFocusUnderline {\r\n    background-color: rgb(110, 92, 52);\r\n}\r\n\r\n.freebirdThemedInput .exportHint {\r\n    color: rgb(110, 92, 52);\r\n}\r\n\r\n.freebirdThemedInput.exportHasError .exportContent.exportUnderline, .freebirdThemedInput.exportHasError .exportContent.exportFocusUnderline {\r\n    background-color: rgb(224, 218, 203);\r\n}\r\n\r\n.freebirdThemedInputDarkerDisabled.isDisabled .exportInput[disabled] {\r\n    color: rgba(0, 0, 0, .87);\r\n}\r\n\r\n.freebirdThemedToggle.isChecked .exportThumb {\r\n    border-color: rgb(110, 92, 52);\r\n    color: rgba(255, 255, 255, 1);\r\n}\r\n\r\n.freebirdThemedToggle.isChecked > .exportTrack {\r\n    border-color: rgb(224, 218, 203);\r\n}\r\n\r\n.freebirdThemedToggle.isCheckedNext > .exportInk, .freebirdThemedToggle.isFocused > .exportInk {\r\n    background-color: rgba(110, 92, 52, 0.15);\r\n}\r\n\r\n.freebirdThemedToggleSwitch {\r\n    border-color: rgb(110, 92, 52);\r\n}\r\n\r\n    .freebirdThemedToggleSwitch.isChecked {\r\n        background-color: rgb(110, 92, 52);\r\n        color: rgba(255, 255, 255, 1);\r\n    }\r\n\r\n.freebirdDarkIconOnThemedBackground {\r\n    display: none;\r\n}\r\n\r\n.ds-hc-white .freebirdDarkIconOnThemedBackground {\r\n    display: block;\r\n}\r\n\r\n.ds-hc-white .freebirdLightIconOnThemedBackground {\r\n    display: none;\r\n}\r\n\r\n.freebirdHeaderMast {\r\n    background-image: url(https://lh3.googleusercontent.com/3DFz23IMjSBGDJx9y6CJp3gSJhTnqJDEn75z3yTiLfPWal2AHQMBtrcUn0jxh9T2yhMCXgNbvg=w989);\r\n    background-size: cover;\r\n    background-position: center;\r\n    color: rgba(255, 255, 255, 1);\r\n}\r\n\r\n.freebirdHeaderMastWithOverlay {\r\n    background-image: linear-gradient(to top, rgba(0, 0, 0, 0) 0%, rgba(0, 0, 0, 0.3) 65%, rgba(0, 0, 0, 0.4) 100%), url(https://lh3.googleusercontent.com/3DFz23IMjSBGDJx9y6CJp3gSJhTnqJDEn75z3yTiLfPWal2AHQMBtrcUn0jxh9T2yhMCXgNbvg=w989);\r\n    background-size: cover;\r\n    background-position: center;\r\n    color: rgba(255, 255, 255, 1);\r\n}\r\n\r\n    .freebirdHeaderMastWithOverlay .freebirdMutedText {\r\n        color: rgba(255, 255, 255, 0.7);\r\n    }\r\n\r\n    .freebirdHeaderMastWithOverlay .freebirdFormTitleInput .exportLabel, .freebirdHeaderMastWithOverlay .exportInput {\r\n        color: rgba(255, 255, 255, 1);\r\n    }\r\n\r\n    .freebirdHeaderMastWithOverlay .exportFocusUnderline {\r\n        background-color: rgba(255, 255, 255, 1);\r\n    }\r\n\r\nhtml:not([style-scope]):not(.style-scope).freebird, body:not([style-scope]):not(.style-scope).freebirdLightBackground {\r\n    background-color: rgb(224, 218, 203);\r\n}\r\n\r\n.freebirdDisclaimerColor, .freebirdDisclaimerColor a {\r\n    color: rgb(100, 100, 100);\r\n}\r\n\r\n.exportSplash {\r\n    background-color: rgb(110, 92, 52);\r\n}\r\n\r\n.freebirdThemedDialog .exportTitleBarFullScreen, .freebirdThemedMobileDialog .exportTitleBarFullScreen, .freebirdThemedDialog .exportTitleBar, .freebirdThemedDialog .exportDialogClose, .freebirdThemedMobileDialog .exportDialogClose {\r\n    background-color: rgb(110, 92, 52);\r\n    color: rgba(255, 255, 255, 1);\r\n    fill: rgba(255, 255, 255, 1);\r\n}\r\n\r\n.freebirdThemedDialog .exportDefaultDialogButton {\r\n    color: rgb(110, 92, 52);\r\n}\r\n\r\n.freebirdThemedDialog.exportMobileDialog .exportDefaultDialogButton, .freebirdThemedMobileDialog.exportMobileDialog .exportDefaultDialogButton {\r\n    color: rgba(255, 255, 255, 1);\r\n}\r\n\r\n.freebirdThemedDialog .exportDefaultDialogButton .exportInk {\r\n    background-image: radial-gradient( circle farthest-side,rgba(110, 92, 52, 0.15),rgba(110, 92, 52, 0.15) 80%,rgba(110, 92, 52, 0) 100% );\r\n}\r\n\r\n.freebirdThemedDialog .exportDefaultDialogButton.isDisabled {\r\n    color: rgba(110, 92, 52, 0.5);\r\n}\r\n\r\n.freebirdThemedDialog .exportDefaultDialogButton.isFocused .exportOverlay {\r\n    background-color: rgba(110, 92, 52, 0.15);\r\n}\r\n\r\n.freebirdThemedDialog .exportTabPageButton {\r\n    background-color: rgb(110, 92, 52);\r\n}\r\n\r\n.freebirdDarkOutline {\r\n    outline-style: solid;\r\n    outline-color: rgb(110, 92, 52);\r\n}\r\n\r\n.freebirdQuizResponse .isDisabled .freebirdDarkOutline {\r\n    outline-color: rgba(0, 0, 0, .54);\r\n}\r\n\r\n.freebirdLabeledControlDarkerDisabled.isDisabled .exportLabel {\r\n    color: rgba(0, 0, 0, .54);\r\n}\r\n\r\n.freebirdLabeledControlDarkerDisabled.isDisabled.isChecked .exportLabel {\r\n    color: rgba(0, 0, 0, .87);\r\n}\r\n\r\n.freebirdLabeledControlDarkerDisabled.isDisabled .exportTooltip svg {\r\n    fill: rgba(0, 0, 0, .54);\r\n}\r\n\r\n.freebirdLabeledControlDarkerDisabled.isDisabled.isChecked .exportTooltip svg {\r\n    fill: rgba(0, 0, 0, .87);\r\n}\r\n\r\n.freebirdThemedText {\r\n    color: rgb(110, 92, 52);\r\n}\r\n\r\n.accentColor {\r\n    color: rgb(226, 178, 79);\r\n}\r\n\r\n.freebirdSolidColor {\r\n    color: rgb(110, 92, 52);\r\n}\r\n\r\n.freebirdSolidBackground {\r\n    background-color: rgb(110, 92, 52);\r\n    color: rgba(255, 255, 255, 1);\r\n}\r\n\r\n.freebirdLightBackground {\r\n    background-color: rgb(224, 218, 203);\r\n}\r\n\r\n.freebirdUltraLightBackgrouand {\r\n    background-color: rgba(110, 92, 52, 0.05);\r\n}\r\n\r\n.freebirdAccentBackground {\r\n    background-color: rgb(226, 178, 79);\r\n    color: rgba(0, 0, 0, 1);\r\n}\r\n\r\n.freebirdLighterBackgroundHover:hover {\r\n    background-color: rgba(110, 92, 52, 0.1);\r\n}\r\n\r\n.freebirdSolidFill {\r\n    fill: rgb(110, 92, 52);\r\n    stroke: rgb(110, 92, 52);\r\n}\r\n\r\n.freebirdSolidBorder {\r\n    border-color: rgb(110, 92, 52);\r\n}\r\n", ""]);

// exports


/***/ }),
/* 18 */
/***/ (function(module, exports, __webpack_require__) {

exports = module.exports = __webpack_require__(2)(undefined);
// imports


// module
exports.push([module.i, "li .glyphicon {\r\n    margin-right: 10px;\r\n}\r\n\r\n/* Highlighting rules for nav menu items */\r\nli.link-active a,\r\nli.link-active a:hover,\r\nli.link-active a:focus {\r\n    background-color: #4189C7;\r\n    color: white;\r\n}\r\n\r\n/* Keep the nav menu independent of scrolling and on top of other items */\r\n.main-nav {\r\n    position: fixed;\r\n    top: 0;\r\n    left: 0;\r\n    right: 0;\r\n    z-index: 1;\r\n}\r\n\r\n@media (min-width: 768px) {\r\n    /* On small screens, convert the nav menu to a vertical sidebar */\r\n    .main-nav {\r\n        height: 100%;\r\n        width: calc(25% - 20px);\r\n    }\r\n    .navbar {\r\n        border-radius: 0px;\r\n        border-width: 0px;\r\n        height: 100%;\r\n    }\r\n    .navbar-header {\r\n        float: none;\r\n    }\r\n    .navbar-collapse {\r\n        border-top: 1px solid #444;\r\n        padding: 0px;\r\n    }\r\n    .navbar ul {\r\n        float: none;\r\n    }\r\n    .navbar li {\r\n        float: none;\r\n        font-size: 15px;\r\n        margin: 6px;\r\n    }\r\n    .navbar li a {\r\n        padding: 10px 16px;\r\n        border-radius: 4px;\r\n    }\r\n    .navbar a {\r\n        /* If a menu item's text is too long, truncate it */\r\n        width: 100%;\r\n        white-space: nowrap;\r\n        overflow: hidden;\r\n        text-overflow: ellipsis;\r\n    }\r\n}\r\n", ""]);

// exports


/***/ }),
/* 19 */
/***/ (function(module, exports) {

module.exports = "<div class='container-fluid'>\r\n    <div class='row'>\r\n        <div class='col-sm-3'>\r\n            <nav-menu></nav-menu>\r\n        </div>\r\n        <div class='col-sm-9 body-content'>\r\n            <router-outlet></router-outlet>\r\n        </div>\r\n    </div>\r\n</div>\r\n";

/***/ }),
/* 20 */
/***/ (function(module, exports) {

module.exports = "<h1>Counter</h1>\r\n\r\n<p>This is a simple example of an Angular component.</p>\r\n\r\n<p>Current count: <strong>{{ currentCount }}</strong></p>\r\n\r\n<button (click)=\"incrementCounter()\">Increment</button>\r\n";

/***/ }),
/* 21 */
/***/ (function(module, exports) {

module.exports = "<head>\r\n    <link rel=\"shortcut icon\" sizes=\"16x16\" href=\"https://ssl.gstatic.com/docs/spreadsheets/forms/favicon_qp2.png\">\r\n    <title>Anmeldung Leapin' Lindy 2018</title>\r\n    <!--<link rel=\"stylesheet\" href=\"https://cdnjs.cloudflare.com/ajax/libs/materialize/0.100.2/css/materialize.min.css\">-->\r\n\r\n    <meta name=\"viewport\" content=\"width=device-width, initial-scale=1\">\r\n    <script type=\"text/javascript\" async=\"\" src=\"https://www.gstatic.com/recaptcha/api2/r20170925162623/recaptcha__de.js\"></script>\r\n    <script type=\"text/javascript\">window.WIZ_global_data = { 'w2btAe': '%.@.\\x2204719177169454866577\\x22,\\x2204719177169454866577\\x22,\\x220\\x22,true\\x5d\\n' };</script>\r\n    <link rel=\"stylesheet\" href=\"https://ssl.gstatic.com/docs/script/css/add-ons1.css\">\r\n    <link rel=\"icon\" sizes=\"192x192\" href=\"//ssl.gstatic.com/docs/forms/device_home/android_192.png\">\r\n    <link rel=\"apple-touch-icon\" sizes=\"120x120\" href=\"//ssl.gstatic.com/docs/forms/device_home/ios_120.png\">\r\n    <link rel=\"apple-touch-icon\" sizes=\"152x152\" href=\"//ssl.gstatic.com/docs/forms/device_home/ios_152.png\">\r\n    <meta name=\"msapplication-TileImage\" content=\"//ssl.gstatic.com/docs/forms/device_home/windows_144.png\">\r\n    <meta name=\"msapplication-TileColor\" content=\"#673ab7\">\r\n    <script src=\"https://www.google.com/recaptcha/api.js\" async=\"\" defer=\"\"></script>\r\n    <script>_docs_flag_initialData = { \"docs-ails\": \"docs_warm\", \"docs-fwds\": \"docs_sdf\", \"docs-crs\": \"docs_crs_nfd\", \"info_params\": { \"token\": \"LgQQ0F4BAAA.6oq-ItlJ8VN4d-d3HMNiUA.6f5aUi43bgAX5umIIBlHMA\" }, \"uls\": \"{\\\"langs\\\":[\\\"de\\\"],\\\"itcs\\\":[],\\\"override\\\":\\\"\\\",\\\"selected\\\":\\\"\\\",\\\"activated\\\":false}\", \"scotty_upload_url\": \"/upload/forms/resumable\", \"docs-enable_feedback_svg\": false, \"enable_feedback\": true, \"docs-fpid\": 713678, \"docs-fbid\": \"ExternalUserData\", \"domain_type\": \"ND\", \"icso\": false, \"docs-obsImUrl\": \"https://ssl.gstatic.com/docs/common/cleardot.gif\", \"docs_oogt\": \"NONE\", \"docs-text-ewf\": true, \"docs-text-ewfird\": false, \"docs-text-wfird\": 10, \"docs-wfsl\": [\"ca\", \"da\", \"de\", \"en\", \"es\", \"fi\", \"fr\", \"it\", \"nl\", \"no\", \"pt\", \"sv\"], \"docs-esfla\": false, \"docs-efpsf\": false, \"docs-evnli\": false, \"docs-emfms\": false, \"docs-edf\": false, \"docs-text-eemfe\": false, \"docs-efsd\": false, \"docs-eidf\": false, \"docs-fse\": false, \"ilcm\": { \"eui\": \"ADFN-cuMPH8Yp8IXBlXK_oqvU7qavLLs44g3qMqfhJkZElhI5pqm1OQVfN78jVa1TnwzGKulmeN9\", \"je\": 1, \"sstu\": 1506725664192000, \"si\": \"CIDs87--y9YCFcVogQodCWcGnw\", \"ei\": [5700920, 5700684, 5701691, 5701614, 5700986, 5700994, 5700180, 5700551, 5700604, 5700912, 5700547, 5700474, 5700828, 5700840, 5700736, 5701381, 5700592, 5701099, 5700908, 5701500, 5701683, 5701397, 5700856, 5701196, 5701832, 5700213, 5702023, 5700450, 5700016, 5700042, 5700720, 5701918, 5701520, 5701300, 5701590, 5701870, 5701360, 5700792, 5700103, 5700873, 5700650, 5700505, 5701892, 5700286, 5700990, 5701804, 5700058, 5701292, 5701844, 5700658, 5701545] }, \"docs-eoi\": false, \"docs-ce\": true, \"docs-hatsfl\": \"https://survey.googleratings.com/wix/p7583646.aspx?ctry\\u0026uilang\\u003dde\\u0026ui\\u003d1\\u0026v1\\u003dND\", \"docs-hatst\": 0, \"docs-egh\": false, \"docs-hatsl\": \"\", \"buildLabel\": \"apps_forms_2017.38-Thu_RC02\", \"docs-show_debug_info\": false, \"ondlburl\": \"//docs.google.com\", \"drive_url\": \"//drive.google.com?authuser\\u003d0\\u0026usp\\u003dforms_web\", \"app_url\": \"https://docs.google.com/forms/?authuser\\u003d0\\u0026usp\\u003dforms_web\", \"docs-mid\": 2048, \"docs-eicd\": false, \"docs-icdmt\": [], \"docs-sup\": \"/forms\", \"docs-seu\": \"https://docs.google.com/forms/d/e/1FAIpQLSe2uZzZU0xKyIOCXPfo6kdklnDnrQf9qipNMaVQAaI-vW7h2w/edit\", \"docs-uptc\": [\"lsrp\", \"noreplica\", \"ths\", \"tam\", \"ntd\", \"app_install_xsrf_token\", \"sle\", \"dl\", \"usp\", \"urp\", \"utm_source\", \"utm_medium\", \"utm_campaign\", \"utm_term\", \"utm_content\"], \"docs-uddn\": \"\", \"docs-udn\": \"\", \"docs-cwsd\": \"https://clients5.google.com\", \"docs-msoil\": \"docs_kansas\", \"docs-tafl\": false, \"docs-hpi\": \"\", \"docs-thtea\": false, \"docs-euctu\": true, \"docs-tdc\": \"[{\\\"id\\\":\\\"0:Reports\\\",\\\"name\\\":\\\"Berichte und Vorschl�ge\\\",\\\"deletedIds\\\":[]},{\\\"id\\\":\\\"0:Letters\\\",\\\"name\\\":\\\"Briefe\\\",\\\"deletedIds\\\":[]},{\\\"id\\\":\\\"0:Brochures\\\",\\\"name\\\":\\\"Brosch�ren und Newsletter\\\",\\\"deletedIds\\\":[]},{\\\"id\\\":\\\"0:Finance\\\",\\\"name\\\":\\\"Finanzen und Buchhaltung\\\",\\\"deletedIds\\\":[]},{\\\"id\\\":\\\"0:Basics\\\",\\\"name\\\":\\\"Grundlagen\\\",\\\"deletedIds\\\":[]},{\\\"id\\\":\\\"0:Calendars\\\",\\\"name\\\":\\\"Kalender und Zeitpl�ne\\\",\\\"deletedIds\\\":[]},{\\\"id\\\":\\\"0:Planners\\\",\\\"name\\\":\\\"Tracker\\\",\\\"deletedIds\\\":[]},{\\\"id\\\":\\\"0:Business\\\",\\\"name\\\":\\\"Vertr�ge und Formulare\\\",\\\"deletedIds\\\":[]},{\\\"id\\\":\\\"Unparented\\\",\\\"name\\\":\\\"Keine Kategorie\\\",\\\"deletedIds\\\":[\\\"0:NoTemplateCategories\\\"]}]\", \"docs-ttt\": 0, \"docs-tcdtc\": \"[]\", \"docs-mtdl\": 500, \"docs-ividtg\": false, \"docs-tdvc\": false, \"docs-tdcp\": 0, \"docs-tmbp\": false, \"docs-hetsdd\": false, \"docs-etao\": true, \"docs-etaogu\": true, \"docs-etaos\": false, \"docs-tintd\": false, \"enable_anonymous_photo_creation\": false, \"docs-al\": [1, 1, 1, 1, 1], \"docs-ndt\": \"Unbenanntes Formular\", \"docs-rpe\": true, \"docs-sfcnidt\": false, \"docs-sfcnidtwi\": false, \"docs-as\": \"\", \"docs-etdimo\": true, \"docs-eifmp\": false, \"docs-mdck\": \"AIzaSyD8OLHtLvDxnjZsBoVq4-_cuwUbKEMa70s\", \"docs-etiff\": false, \"docs-eihhc\": false, \"docs-spfe\": true, \"docs-mriim\": 1800000, \"docs-eccbs\": false, \"docos-sosj\": false, \"docs-rlmp\": false, \"docs-mmpt\": 15000, \"docs-erd\": false, \"docs-erfar\": false, \"docs-ensb\": false, \"docs-ddts\": false, \"docs-uootuns\": false, \"docs-amawso\": false, \"docs-mdso\": false, \"docs-ofmpp\": false, \"docs-anlpfdo\": false, \"docs-depquafr\": false, \"docs-pid\": \"114830392522716751150\", \"ecid\": true, \"docs-emo\": false, \"docs-eos\": true, \"docs-pedd\": true, \"docs-eir\": false, \"docs-edll\": false, \"docs-eivu\": false, \"docs-dc-ca\": false, \"server_time_ms\": 1506725664205, \"gaia_session_id\": \"0\", \"app-bc\": \"#673ab7\", \"enable_iframed_embed_api\": true, \"docs-fut\": \"//drive.google.com?authuser\\u003d0\\u0026usp\\u003dforms_web#folders/{folderId}\", \"esid\": true, \"esubid\": false, \"docs-etbs\": true, \"docs-usp\": \"forms_web\", \"docs-isb\": true, \"docs-enct\": false, \"docs-dc\": false, \"docs-mtrb1c\": \"\", \"docs-mtrb2c\": \"\", \"docs-mtrb3c\": \"\", \"docs-anddc\": true, \"docs-eflimt\": true, \"docs-mcssa\": false, \"docs-mib\": 52428800, \"docs-mip\": 25000000, \"docs-cp\": true, \"docs-ssi\": false, \"docs-dom\": false, \"docs-fwd\": false, \"docs-dwc\": false, \"docs-tdd\": false, \"enable_kennedy\": true, \"docs-gth\": \"Google Formulare-Startbildschirm �ffnen\", \"docs-ema\": false, \"docs-emi\": false, \"docs-epiot\": true, \"projector_view_url\": \"https://drive.google.com/file/d/e/1FAIpQLSe2uZzZU0xKyIOCXPfo6kdklnDnrQf9qipNMaVQAaI-vW7h2w/view?usp\\u003ddocs_web\", \"docs-eopiiv2\": false, \"opendv\": true, \"onePickImportDocumentUrl\": \"\", \"opmbs\": 52428800, \"opmpd\": 5000, \"opbu\": \"https://docs.google.com/picker\", \"opru\": \"https://docs.google.com/relay.html\", \"opdu\": true, \"opccp\": false, \"ophi\": \"trix_forms\", \"opst\": \"000770F203A52E9F3644759B0CEFBFB01CD75F2B85E8C12123::1506725664208\", \"opuci\": \"\", \"docs-to\": \"https://docs.google.com\", \"maestro_domain\": \"https://script.google.com\", \"maestro_container_token\": \"ACjPJvF4mrKY-NkKZWXrRQdW2EiwsvnfNzmP2hHi0iwCMN4xneXPpCwTAdxnie1ITINJEyQh8AfRvRc5bSAKXKIcCwTvP-wB0ani9MKNFR5iyzCGCHw\", \"maestro_script_editor_uri\": \"https://script.google.com/macros/start?mid\\u003dACjPJvF4mrKY-NkKZWXrRQdW2EiwsvnfNzmP2hHi0iwCMN4xneXPpCwTAdxnie1ITINJEyQh8AfRvRc5bSAKXKIcCwTvP-wB0ani9MKNFR5iyzCGCHw\\u0026uiv\\u003d2\", \"maestro_new_project_uri\": \"https://script.google.com/macros/create?mid\\u003dACjPJvF4mrKY-NkKZWXrRQdW2EiwsvnfNzmP2hHi0iwCMN4xneXPpCwTAdxnie1ITINJEyQh8AfRvRc5bSAKXKIcCwTvP-wB0ani9MKNFR5iyzCGCHw\\u0026uiv\\u003d2\", \"maestro_script_gallery_uri\": \"https://docs.google.com/macros/scriptGalleryPanel?mid\\u003dACjPJvF4mrKY-NkKZWXrRQdW2EiwsvnfNzmP2hHi0iwCMN4xneXPpCwTAdxnie1ITINJEyQh8AfRvRc5bSAKXKIcCwTvP-wB0ani9MKNFR5iyzCGCHw\\u0026uiv\\u003d2\", \"maestro_script_manager_uri\": \"https://script.google.com/macros/scriptManagerPanel?mid\\u003dACjPJvF4mrKY-NkKZWXrRQdW2EiwsvnfNzmP2hHi0iwCMN4xneXPpCwTAdxnie1ITINJEyQh8AfRvRc5bSAKXKIcCwTvP-wB0ani9MKNFR5iyzCGCHw\\u0026uiv\\u003d2\", \"enable_maestro\": true, \"docs-emae\": true, \"mae-cwssw\": false, \"mae-aoeba\": true, \"mae-esme\": false, \"mae-seitd\": true, \"docs-mhea\": false, \"docs-hes\": true, \"docs-hecud\": true, \"docs-heoi\": false, \"docs-hue\": \"mathias.minder@gmail.com\", \"docs-offline-uiaffd\": false, \"docs-offline-uoia\": false, \"docs-hetd\": true, \"docs-hhso\": false, \"docs-huogmb\": true, \"docs-hoec\": false, \"docs-hosui\": false, \"docs-huso\": false, \"docs-hevq\": false, \"docs-hecom\": true, \"docs-heftu\": false, \"docs-cci\": \"PROD\", \"docs-caru\": \"https://clients6.google.com\", \"docs-cbau\": \"https://drive.google.com\", \"docs-cfru\": \"https://lh3.google.com\", \"docs-ctcu\": \"https://client-channel.google.com/client-channel/client\", \"docs-ctsu\": \"https://clients4.google.com/invalidation/lcs/client\", \"docs-cpv\": 0, \"docs-cpf\": \"c16f490e3d087f1f\", \"docs-hasid\": \"Forms\", \"docs-hufcm\": false, \"docs-epdf\": false, \"docs-hdck\": \"AIzaSyD3SxAFA7YuMzXbJHCPKlNCHD-myTZZHwQ\", \"docs-cpkl\": [\"ABisA0VWJrUE1OejhDAWh6zYrZLwX1rFPtcGarp47ysVUGjfd+k2Muyu5d+ijrs6TIVwPapTyUkf\"], \"docs-hucs\": true, \"docs-hunca\": false, \"docs-hdod\": \"docs.google.com\", \"docs-hesd\": false, \"jobset\": \"prod\", \"docs-eafn\": false, \"docs-nad\": \"\", \"docs-epcc\": false, \"docs-dlpe\": false, \"docs-erhur\": false, \"docs-enr\": false, \"docs-enrhmnrp\": false, \"docs-ediit\": false, \"docs-emddi\": false, \"docs-eemnr\": false, \"docs-errtv\": false, \"enable_omnibox_help_menu\": true, \"enable_omnibox\": true, \"docs-sticky_view_mode\": true, \"docs-se\": false, \"docs-ebcrsct\": false, \"docs-iror\": false, \"docs-hefad\": false, \"docs-eopfo\": true, \"docs-eopfov2\": false, \"xdbcmUri\": \"https://docs.google.com/forms/xdbcm.html\", \"xdbcfAllowXpc\": true, \"docs-corsbc\": false, \"xdbcfAllowHostNamePrefix\": true, \"docs-spdy\": false, \"docs-csi-reporting-uri-override\": \"\", \"enable_csi\": true, \"csi_service_name\": \"freebird\", \"lwqxc-dxkn\": false, \"lwqxc-dmy\": false, \"zpgp\": \"fhykjaykjkygfphhozgenjsylxyfltyxgajsbpuygqfxcjisbagifvbtehcsnsudgtszjcuflujjwwlwabblqrs\", \"lwbfc_ezbhzyjyvo_slwjvxs\": \"/c/\", \"ycfrots_yaspuda_roukwnxmoc\": 0.3, \"gxowi_sjzp_judnqseuhs\": 0.5, \"gxowi_qlbp_aefm_roukwnxmoc\": 0.3, \"lwqx_mhyupq_lkaqdnafpvxr_bhgxif_bsv\": \"https://docs.google.com/picker?protocol\\u003dgadgets\\u0026relayUrl\\u003dhttps://docs.google.com/relay.html\\u0026hostId\\u003dfusiontables-form-linker\\u0026title\\u003dW%C3%A4hlen+Sie+eine+Fusion+Table+aus,+in+die+die+Formularantworten+kopiert+werden+sollen.\\u0026hl\\u003dde\\u0026authuser\\u003d0\\u0026newDriveView\\u003dtrue\\u0026origin\\u003dhttps://docs.google.com\\u0026st\\u003d000770F203884BB8878BB856E858296B209BC23CD211580BCF::1506725664219\\u0026driveGridViewSwitcherHidden\\u003dtrue\\u0026nav\\u003d((%22tables%22),(%22recently-picked%22))\", \"lwqx_mhyupq_gbt_hpyjonlgqcs_pczjvh_ntb\": \"https://docs.google.com/picker?protocol\\u003dgadgets\\u0026relayUrl\\u003dhttps://docs.google.com/relay.html\\u0026hostId\\u003dspreadsheet-form-linker\\u0026title\\u003dW%C3%A4hlen+Sie+eine+Tabelle+aus,+in+die+die+Formularantworten+kopiert+werden+sollen.\\u0026hl\\u003dde\\u0026authuser\\u003d0\\u0026newDriveView\\u003dtrue\\u0026origin\\u003dhttps://docs.google.com\\u0026st\\u003d000770F203884BB8878BB856E858296B209BC23CD211580BCF::1506725664219\\u0026nav\\u003d((%22spreadsheets%22,null,%7B%22mimeTypes%22:%22application/vnd.google-apps.spreadsheet,application/vnd.google-apps.ritz%22%7D))\", \"ouozbu_erpll_tqiaon_ibe\": \"https://docs.google.com/picker?protocol\\u003dgadgets\\u0026relayUrl\\u003dhttps://docs.google.com/relay.html\\u0026hostId\\u003dimport-theme\\u0026title\\u003dDesign+von+bestehendem+Formular+kopieren\\u0026hl\\u003dde\\u0026authuser\\u003d0\\u0026newDriveView\\u003dtrue\\u0026origin\\u003dhttps://docs.google.com\\u0026st\\u003d000770F203884BB8878BB856E858296B209BC23CD211580BCF::1506725664219\\u0026nav\\u003d((%22forms%22),(%22recently-picked%22))\", \"cma_ppme_ohhreo_jrs\": \"https://docs.google.com/picker?protocol\\u003dgadgets\\u0026relayUrl\\u003dhttps://docs.google.com/relay.html\\u0026hostId\\u003dtrix_forms-fonts\\u0026title\\u003dSchriftarten\\u0026hl\\u003dde\\u0026authuser\\u003d0\\u0026newDriveView\\u003dtrue\\u0026origin\\u003dhttps://docs.google.com\\u0026st\\u003d000770F203884BB8878BB856E858296B209BC23CD211580BCF::1506725664219\\u0026navHidden\\u003dtrue\\u0026multiselectEnabled\\u003dtrue\\u0026selectButtonLabel\\u003dOK\\u0026nav\\u003d((%22fonts%22))\", \"jzhgo_taqczkk_abdrhls_ehzi\": \"https://www.google.com/settings/storage\", \"oa_eobl_nchck_nwgm\": false, \"lwqxc-rqen\": true, \"lwqxc-dov\": false, \"lwqxc-dbda\": true, \"lwqxc-dxmzo\": true, \"lwqxc-dnka\": false, \"lwqxc-doz\": false, \"lwqxc-dmlq\": true, \"lwqxc-dmc\": true, \"lwqxc-ddka\": true, \"lwqxc-ddklh\": true, \"lwqxc-ddkw\": false, \"lwqxc-dxcl\": true, \"lwqxc-dbbn\": false, \"lwqxc-da\": true, \"lwqxc-ddb\": true, \"lwqxc-dzqm\": true, \"lwqxc-dlf\": true, \"lwqxc-rgm\": 0.1, \"lwqxc-rgptb\": 0.7, \"lwqxc-ddk\": false, \"lwqxc-dweo\": true, \"lwqxc-dbhb\": true, \"lwqxc-dlaip\": true, \"lwqxc-dywe\": false, \"lwqxc-dddtmhz\": false, \"lwqxc-dldzr\": false, \"lwqxc-dnm\": true, \"lwqxc-dcp\": false, \"lwqxc-drq\": false, \"lwqxc-dmpf\": false, \"lwqxc-cdd\": \"Unbenannte Umfrage\", \"lwqxc-dyex\": true, \"lwqxc-dqdd\": false, \"lwqxc-dnc\": false, \"lwqxc-dxcqd\": true, \"lwqxc-keg\": true, \"lwqxc-drk\": true, \"googlesystem_blogspot_banlevel\": \"http://goo.gl/vqaya\", \"docs-idu\": false, \"docs-esdur\": true, \"docs-eobs\": false, \"docs-eobsp\": false, \"docs-dcr\": false };</script>\r\n    <base target=\"_blank\">\r\n    <meta property=\"og:title\" content=\"Anmeldung Leapin' Lindy 2018\">\r\n    <meta property=\"og:type\" content=\"article\">\r\n    <meta property=\"og:site_name\" content=\"Google Docs\">\r\n    <meta property=\"og:url\" content=\"https://docs.google.com/forms/d/e/1FAIpQLSe2uZzZU0xKyIOCXPfo6kdklnDnrQf9qipNMaVQAaI-vW7h2w/viewform?usp=embed_facebook\">\r\n    <meta property=\"og:image\" content=\"https://lh5.googleusercontent.com/rEU-biVfnw1p7ErJibaFdYOIe9gMotMlNl_bwFby_V-u0yB419CjZHR_rTAOsa8kxQI=w1200-h630-p\">\r\n    <meta property=\"og:image:width\" content=\"200\">\r\n    <meta property=\"og:image:height\" content=\"200\">\r\n    <style>\r\n        @import url('https://fonts.googleapis.com/css?lang=de&family=Product+Sans|Roboto:400,700');\r\n\r\n        .gb_rd {\r\n            font: 13px/27px Roboto,RobotoDraft,Arial,sans-serif;\r\n            z-index: 986\r\n        }\r\n\r\n        @-webkit-keyframes gb__a {\r\n            0% {\r\n                opacity: 0\r\n            }\r\n\r\n            50% {\r\n                opacity: 1\r\n            }\r\n        }\r\n\r\n        @keyframes gb__a {\r\n            0% {\r\n                opacity: 0\r\n            }\r\n\r\n            50% {\r\n                opacity: 1\r\n            }\r\n        }\r\n\r\n        a.gb_Ba {\r\n            border: none;\r\n            color: #4285f4;\r\n            cursor: default;\r\n            font-weight: bold;\r\n            outline: none;\r\n            position: relative;\r\n            text-align: center;\r\n            text-decoration: none;\r\n            text-transform: uppercase;\r\n            white-space: nowrap;\r\n            -webkit-user-select: none\r\n        }\r\n\r\n            a.gb_Ba:hover:after, a.gb_Ba:focus:after {\r\n                background-color: rgba(0,0,0,.12);\r\n                content: '';\r\n                height: 100%;\r\n                left: 0;\r\n                position: absolute;\r\n                top: 0;\r\n                width: 100%\r\n            }\r\n\r\n            a.gb_Ba:hover, a.gb_Ba:focus {\r\n                text-decoration: none\r\n            }\r\n\r\n            a.gb_Ba:active {\r\n                background-color: rgba(153,153,153,.4);\r\n                text-decoration: none\r\n            }\r\n\r\n        a.gb_Ca {\r\n            background-color: #4285f4;\r\n            color: #fff\r\n        }\r\n\r\n            a.gb_Ca:active {\r\n                background-color: #0043b2\r\n            }\r\n\r\n        .gb_Da {\r\n            -webkit-box-shadow: 0 1px 1px rgba(0,0,0,.16);\r\n            box-shadow: 0 1px 1px rgba(0,0,0,.16)\r\n        }\r\n\r\n        .gb_Ba, .gb_Ca, .gb_Ea, .gb_Fa {\r\n            display: inline-block;\r\n            line-height: 28px;\r\n            padding: 0 12px;\r\n            -webkit-border-radius: 2px;\r\n            border-radius: 2px\r\n        }\r\n\r\n        .gb_Ea {\r\n            background: #f8f8f8;\r\n            border: 1px solid #c6c6c6\r\n        }\r\n\r\n        .gb_Fa {\r\n            background: #f8f8f8\r\n        }\r\n\r\n        .gb_Ea, #gb a.gb_Ea.gb_Ea, .gb_Fa {\r\n            color: #666;\r\n            cursor: default;\r\n            text-decoration: none\r\n        }\r\n\r\n        #gb a.gb_Fa.gb_Fa {\r\n            cursor: default;\r\n            text-decoration: none\r\n        }\r\n\r\n        .gb_Fa {\r\n            border: 1px solid #4285f4;\r\n            font-weight: bold;\r\n            outline: none;\r\n            background: #4285f4;\r\n            background: -webkit-linear-gradient(top,#4387fd,#4683ea);\r\n            background: linear-gradient(top,#4387fd,#4683ea);\r\n            filter: progid:DXImageTransform.Microsoft.gradient(startColorstr=#4387fd,endColorstr=#4683ea,GradientType=0)\r\n        }\r\n\r\n        #gb a.gb_Fa.gb_Fa {\r\n            color: #fff\r\n        }\r\n\r\n        .gb_Fa:hover {\r\n            -webkit-box-shadow: 0 1px 0 rgba(0,0,0,.15);\r\n            box-shadow: 0 1px 0 rgba(0,0,0,.15)\r\n        }\r\n\r\n        .gb_Fa:active {\r\n            -webkit-box-shadow: inset 0 2px 0 rgba(0,0,0,.15);\r\n            box-shadow: inset 0 2px 0 rgba(0,0,0,.15);\r\n            background: #3c78dc;\r\n            background: -webkit-linear-gradient(top,#3c7ae4,#3f76d3);\r\n            background: linear-gradient(top,#3c7ae4,#3f76d3);\r\n            filter: progid:DXImageTransform.Microsoft.gradient(startColorstr=#3c7ae4,endColorstr=#3f76d3,GradientType=0)\r\n        }\r\n\r\n        .gb_5a {\r\n            display: none !important\r\n        }\r\n\r\n        .gb_6a {\r\n            visibility: hidden\r\n        }\r\n\r\n        .gb_Zc {\r\n            display: inline-block;\r\n            vertical-align: middle\r\n        }\r\n\r\n        .gb_Fc {\r\n            position: relative\r\n        }\r\n\r\n        .gb_b {\r\n            display: inline-block;\r\n            outline: none;\r\n            vertical-align: middle;\r\n            -webkit-border-radius: 2px;\r\n            border-radius: 2px;\r\n            -webkit-box-sizing: border-box;\r\n            box-sizing: border-box;\r\n            height: 40px;\r\n            width: 40px;\r\n            color: #000;\r\n            cursor: pointer;\r\n            text-decoration: none\r\n        }\r\n\r\n        #gb#gb a.gb_b {\r\n            color: #000;\r\n            cursor: pointer;\r\n            text-decoration: none\r\n        }\r\n\r\n        .gb_lb {\r\n            border-color: transparent;\r\n            border-bottom-color: #fff;\r\n            border-style: dashed dashed solid;\r\n            border-width: 0 8.5px 8.5px;\r\n            display: none;\r\n            position: absolute;\r\n            left: 11.5px;\r\n            top: 43px;\r\n            z-index: 1;\r\n            height: 0;\r\n            width: 0;\r\n            -webkit-animation: gb__a .2s;\r\n            animation: gb__a .2s\r\n        }\r\n\r\n        .gb_mb {\r\n            border-color: transparent;\r\n            border-style: dashed dashed solid;\r\n            border-width: 0 8.5px 8.5px;\r\n            display: none;\r\n            position: absolute;\r\n            left: 11.5px;\r\n            z-index: 1;\r\n            height: 0;\r\n            width: 0;\r\n            -webkit-animation: gb__a .2s;\r\n            animation: gb__a .2s;\r\n            border-bottom-color: #ccc;\r\n            border-bottom-color: rgba(0,0,0,.2);\r\n            top: 42px\r\n        }\r\n\r\n        x:-o-prefocus, div.gb_mb {\r\n            border-bottom-color: #ccc\r\n        }\r\n\r\n        .gb_fa {\r\n            background: #fff;\r\n            border: 1px solid #ccc;\r\n            border-color: rgba(0,0,0,.2);\r\n            color: #000;\r\n            -webkit-box-shadow: 0 2px 10px rgba(0,0,0,.2);\r\n            box-shadow: 0 2px 10px rgba(0,0,0,.2);\r\n            display: none;\r\n            outline: none;\r\n            overflow: hidden;\r\n            position: absolute;\r\n            right: 8px;\r\n            top: 56px;\r\n            -webkit-animation: gb__a .2s;\r\n            animation: gb__a .2s;\r\n            -webkit-border-radius: 2px;\r\n            border-radius: 2px;\r\n            -webkit-user-select: text\r\n        }\r\n\r\n        .gb_Zc.gb_g .gb_lb, .gb_Zc.gb_g .gb_mb, .gb_Zc.gb_g .gb_fa, .gb_g.gb_fa {\r\n            display: block\r\n        }\r\n\r\n        .gb_Zc.gb_g.gb_kf .gb_lb, .gb_Zc.gb_g.gb_kf .gb_mb {\r\n            display: none\r\n        }\r\n\r\n        .gb_lf {\r\n            position: absolute;\r\n            right: 8px;\r\n            top: 56px;\r\n            z-index: -1\r\n        }\r\n\r\n        .gb_9a .gb_lb, .gb_9a .gb_mb, .gb_9a .gb_fa {\r\n            margin-top: -10px\r\n        }\r\n\r\n        .gb_Zc:first-child, #gbsfw:first-child + .gb_Zc {\r\n            padding-left: 4px\r\n        }\r\n\r\n        .gb_uc {\r\n            position: relative\r\n        }\r\n\r\n        .gb_6d .gb_uc, .gb_Xd .gb_uc {\r\n            float: right\r\n        }\r\n\r\n        .gb_b {\r\n            padding: 8px;\r\n            cursor: pointer\r\n        }\r\n\r\n            .gb_xe button:focus svg, .gb_rc .gb_yb:not(.gb_Ba):focus img, .gb_b:focus {\r\n                background-color: rgba(0,0,0,0.20);\r\n                outline: none;\r\n                -webkit-border-radius: 50%;\r\n                border-radius: 50%\r\n            }\r\n\r\n        .gb_Zc {\r\n            padding: 4px\r\n        }\r\n\r\n        .gb_fa {\r\n            z-index: 991;\r\n            line-height: normal\r\n        }\r\n\r\n            .gb_fa.gb_ye {\r\n                left: 8px;\r\n                right: auto\r\n            }\r\n\r\n        @media (max-width:350px) {\r\n            .gb_fa.gb_ye {\r\n                left: 0\r\n            }\r\n        }\r\n\r\n        .gb_1b {\r\n            display: inline-block;\r\n            position: relative;\r\n            top: 2px;\r\n            -webkit-user-select: none\r\n        }\r\n\r\n        .gb_ud .gb_Xa {\r\n            min-width: 74px\r\n        }\r\n\r\n        .gb_ke .gb_1b {\r\n            display: none\r\n        }\r\n\r\n        .gb_ud .gb_2b {\r\n            line-height: normal;\r\n            position: relative;\r\n            padding-left: 16px\r\n        }\r\n\r\n        .gb_4b .gb_vc:before {\r\n            content: url('https://www.gstatic.com/images/branding/googlelogo/svg/googlelogo_clr_74x24px.svg');\r\n            display: inline-block;\r\n            height: 24px;\r\n            width: 74px\r\n        }\r\n\r\n        .gb_4b .gb_vc {\r\n            height: 24px;\r\n            width: 74px\r\n        }\r\n\r\n        .gb_4b {\r\n            direction: ltr\r\n        }\r\n\r\n            .gb_4b .gb_vc, .gb_4b {\r\n                display: inline-block;\r\n                vertical-align: middle\r\n            }\r\n\r\n                .gb_4b .gb_vc, .gb_4b.gb_le, .gb_4b:not(.gb_le):not(:focus) {\r\n                    outline: none\r\n                }\r\n\r\n        .gb_Wa {\r\n            display: inline-block;\r\n            vertical-align: middle\r\n        }\r\n\r\n        .gb_7b {\r\n            border: none;\r\n            display: block;\r\n            visibility: hidden\r\n        }\r\n\r\n        .gb_2d .gb_4b .gb_vc:before {\r\n            content: url('https://www.gstatic.com/images/branding/googlelogo/svg/googlelogo_light_clr_74x24px.svg')\r\n        }\r\n\r\n        .gb_1d .gb_4b .gb_vc:before {\r\n            content: url('https://www.gstatic.com/images/branding/googlelogo/svg/googlelogo_dark_clr_74x24px.svg')\r\n        }\r\n\r\n        .gb_Wa {\r\n            background-repeat: no-repeat\r\n        }\r\n\r\n        .gb_9d {\r\n            display: inline-block;\r\n            font-family: 'Product Sans',Arial,sans-serif;\r\n            font-size: 22px;\r\n            line-height: 24px;\r\n            padding-left: 8px;\r\n            position: relative;\r\n            top: -1.5px;\r\n            vertical-align: middle\r\n        }\r\n\r\n        .gb_5d.gb_9d {\r\n            opacity: .54\r\n        }\r\n\r\n        .gb_8d:focus .gb_9d {\r\n            text-decoration: underline\r\n        }\r\n\r\n        .gb_da .gb_b, .gb_ea .gb_da .gb_b {\r\n            background-position: -64px -29px\r\n        }\r\n\r\n        .gb_X .gb_da .gb_b {\r\n            background-position: -29px -29px;\r\n            opacity: 1\r\n        }\r\n\r\n        .gb_da .gb_b, .gb_da .gb_b:hover, .gb_da .gb_b:focus {\r\n            opacity: 1\r\n        }\r\n\r\n        .gb_sd {\r\n            display: none\r\n        }\r\n\r\n        .gb_Lc {\r\n            color: inherit;\r\n            font-size: 22px;\r\n            font-weight: 400;\r\n            line-height: 48px;\r\n            overflow: hidden;\r\n            padding-left: 16px;\r\n            position: relative;\r\n            text-overflow: ellipsis;\r\n            vertical-align: middle;\r\n            top: 2px;\r\n            white-space: nowrap;\r\n            -webkit-flex: 1 1 auto;\r\n            flex: 1 1 auto\r\n        }\r\n\r\n        .gb_rc.gb_9b .gb_Mc {\r\n            position: relative;\r\n            top: -1px\r\n        }\r\n\r\n        .gb_rc {\r\n            min-width: 320px;\r\n            position: relative;\r\n            -webkit-transition: box-shadow 250ms;\r\n            transition: box-shadow 250ms\r\n        }\r\n\r\n            .gb_rc.gb_td .gb_sc {\r\n                display: none\r\n            }\r\n\r\n            .gb_rc.gb_td .gb_ud {\r\n                height: 56px\r\n            }\r\n\r\n        header.gb_rc {\r\n            display: block\r\n        }\r\n\r\n        .gb_rc svg {\r\n            fill: currentColor\r\n        }\r\n\r\n        .gb_vd {\r\n            position: fixed;\r\n            top: 0;\r\n            width: 100%\r\n        }\r\n\r\n        .gb_wd {\r\n            -webkit-box-shadow: 0 4px 5px 0 rgba(0,0,0,0.14),0 1px 10px 0 rgba(0,0,0,0.12),0 2px 4px -1px rgba(0,0,0,0.2);\r\n            box-shadow: 0 4px 5px 0 rgba(0,0,0,0.14),0 1px 10px 0 rgba(0,0,0,0.12),0 2px 4px -1px rgba(0,0,0,0.2)\r\n        }\r\n\r\n        .gb_xd {\r\n            height: 64px\r\n        }\r\n\r\n        .gb_rc:not(.gb_fc) .gb_Oc.gb_Pc, .gb_rc:not(.gb_fc) .gb_nd {\r\n            display: none !important\r\n        }\r\n\r\n        .gb_ud {\r\n            -webkit-box-sizing: border-box;\r\n            box-sizing: border-box;\r\n            position: relative;\r\n            width: 100%;\r\n            display: -webkit-box;\r\n            display: -moz-box;\r\n            display: -ms-flexbox;\r\n            display: -webkit-flex;\r\n            display: flex;\r\n            -webkit-box-pack: space-between;\r\n            -webkit-justify-content: space-between;\r\n            justify-content: space-between;\r\n            min-width: -webkit-min-content;\r\n            min-width: -moz-min-content;\r\n            min-width: -ms-min-content;\r\n            min-width: min-content\r\n        }\r\n\r\n        .gb_rc:not(.gb_9b) .gb_ud {\r\n            padding: 8px\r\n        }\r\n\r\n        .gb_rc.gb_yd .gb_ud {\r\n            -webkit-flex: 1 0 auto;\r\n            flex: 1 0 auto\r\n        }\r\n\r\n        .gb_rc.gb_9b .gb_ud {\r\n            padding: 4px;\r\n            min-width: 0\r\n        }\r\n\r\n        .gb_sc {\r\n            height: 48px;\r\n            vertical-align: middle;\r\n            white-space: nowrap;\r\n            -webkit-box-align: center;\r\n            -webkit-align-items: center;\r\n            align-items: center;\r\n            display: -webkit-box;\r\n            display: -moz-box;\r\n            display: -ms-flexbox;\r\n            display: -webkit-flex;\r\n            display: flex;\r\n            -webkit-user-select: none\r\n        }\r\n\r\n        .gb_Ad > .gb_sc {\r\n            display: table-cell;\r\n            width: 100%\r\n        }\r\n\r\n        .gb_sc.gb_Bd:not(.gb_Cd) .gb_Dd {\r\n            padding-left: 16px\r\n        }\r\n\r\n        .gb_sc.gb_Ed.gb_Bd:not(.gb_Cd) .gb_Dd, .gb_sc.gb_Fd:not(.gb_Cd) .gb_Dd {\r\n            padding-right: 16px\r\n        }\r\n\r\n        .gb_sc:not(.gb_Cd) .gb_Dd {\r\n            width: 100%;\r\n            -webkit-flex: 1 1 auto;\r\n            flex: 1 1 auto\r\n        }\r\n\r\n        .gb_Dd.gb_6a {\r\n            display: none\r\n        }\r\n\r\n        .gb_Hd.gb_Id > .gb_Jd {\r\n            min-width: initial !important;\r\n            min-width: auto !important\r\n        }\r\n\r\n        .gb_Kd {\r\n            padding-right: 32px;\r\n            -webkit-box-sizing: border-box;\r\n            box-sizing: border-box;\r\n            -webkit-flex: 1 0 auto;\r\n            flex: 1 0 auto\r\n        }\r\n\r\n            .gb_Kd.gb_Ld {\r\n                padding-right: 0\r\n            }\r\n\r\n        .gb_rc.gb_9b .gb_Kd:not(.gb_Md) {\r\n            -webkit-flex: 1 1 auto;\r\n            flex: 1 1 auto;\r\n            overflow: hidden\r\n        }\r\n\r\n        .gb_rc.gb_9b .gb_Hd:not(.gb_Md) {\r\n            -webkit-flex: 0 0 auto;\r\n            flex: 0 0 auto\r\n        }\r\n\r\n        .gb_Hd {\r\n            -webkit-flex: 1 1 100%;\r\n            flex: 1 1 100%\r\n        }\r\n\r\n        .gb_Nd, .gb_Od:not(.gb_Pd):not(.gb_Id).gb_Qd {\r\n            -webkit-box-pack: flex-end;\r\n            -webkit-justify-content: flex-end;\r\n            justify-content: flex-end\r\n        }\r\n\r\n        .gb_Od:not(.gb_Pd):not(.gb_Id) {\r\n            -webkit-box-pack: center;\r\n            -webkit-justify-content: center;\r\n            justify-content: center\r\n        }\r\n\r\n            .gb_Od:not(.gb_Pd):not(.gb_Id).gb_Rd, .gb_Od:not(.gb_Pd):not(.gb_Id).gb_Sd {\r\n                -webkit-box-pack: flex-start;\r\n                -webkit-justify-content: flex-start;\r\n                justify-content: flex-start\r\n            }\r\n\r\n        .gb_Hd.gb_Pd, .gb_Hd.gb_Id {\r\n            -webkit-box-pack: space-between;\r\n            -webkit-justify-content: space-between;\r\n            justify-content: space-between\r\n        }\r\n\r\n        .gb_Hd > :only-child {\r\n            display: inline-block\r\n        }\r\n\r\n        .gb_tc.gb_Td.gb_Ud {\r\n            padding-left: 4px\r\n        }\r\n\r\n        .gb_tc.gb_Td.gb_Vd {\r\n            padding-left: 0\r\n        }\r\n\r\n        .gb_rc.gb_9b .gb_tc.gb_Td.gb_Vd {\r\n            padding-left: 4px;\r\n            padding-right: 0\r\n        }\r\n\r\n        .gb_Ud {\r\n            display: inline\r\n        }\r\n\r\n        .gb_tc.gb_Td {\r\n            -webkit-box-sizing: border-box;\r\n            box-sizing: border-box;\r\n            padding-left: 32px;\r\n            -webkit-flex: 0 0 auto;\r\n            flex: 0 0 auto;\r\n            -webkit-box-pack: flex-end;\r\n            -webkit-justify-content: flex-end;\r\n            justify-content: flex-end\r\n        }\r\n\r\n        .gb_Lc {\r\n            display: inline-block\r\n        }\r\n\r\n        .gb_tc {\r\n            height: 48px;\r\n            line-height: normal;\r\n            padding: 0 4px\r\n        }\r\n\r\n        .gb_Xd {\r\n            height: 48px\r\n        }\r\n\r\n        .gb_rc.gb_Xd {\r\n            min-width: initial;\r\n            min-width: auto\r\n        }\r\n\r\n        .gb_Xd .gb_tc {\r\n            float: right\r\n        }\r\n\r\n        .gb_Zd {\r\n            font-size: 14px;\r\n            max-width: 200px;\r\n            overflow: hidden;\r\n            padding: 0 12px;\r\n            text-overflow: ellipsis;\r\n            white-space: nowrap;\r\n            -webkit-user-select: text\r\n        }\r\n\r\n        .gb_rc {\r\n            color: black\r\n        }\r\n\r\n        .gb_0d {\r\n            background-color: #fff;\r\n            -webkit-transition: background-color .4s;\r\n            transition: background-color .4s\r\n        }\r\n\r\n        .gb_1d {\r\n            color: black;\r\n            background-color: #e0e0e0\r\n        }\r\n\r\n        .gb_2d {\r\n            color: white;\r\n            background-color: #616161\r\n        }\r\n\r\n        .gb_rc a, .gb_dc a {\r\n            color: inherit\r\n        }\r\n\r\n        .gb_rc svg, .gb_dc svg {\r\n            color: black;\r\n            opacity: .54\r\n        }\r\n\r\n        .gb_2d svg {\r\n            color: white;\r\n            opacity: 1\r\n        }\r\n\r\n        .gb_3d:hover, .gb_3d:focus, .gb_4d:hover, .gb_4d:focus {\r\n            opacity: .85\r\n        }\r\n\r\n        .gb_5d {\r\n            color: inherit;\r\n            opacity: 1;\r\n            text-rendering: optimizeLegibility;\r\n            -webkit-font-smoothing: antialiased\r\n        }\r\n\r\n        .gb_2d .gb_5d, .gb_1d .gb_5d {\r\n            opacity: 1\r\n        }\r\n\r\n        .gb_6d > * {\r\n            display: block;\r\n            min-height: 48px\r\n        }\r\n\r\n        .gb_rc.gb_9b .gb_6d > * {\r\n            padding-top: 4px;\r\n            padding-bottom: 4px;\r\n            padding-left: 16px\r\n        }\r\n\r\n        .gb_rc:not(.gb_9b) .gb_6d > * {\r\n            padding-top: 8px;\r\n            padding-bottom: 8px;\r\n            padding-left: 24px\r\n        }\r\n\r\n        .gb_rc:not(.gb_9b) .gb_Kd .gb_1b {\r\n            -webkit-box-align: center;\r\n            -webkit-align-items: center;\r\n            align-items: center;\r\n            display: -webkit-box;\r\n            display: -moz-box;\r\n            display: -ms-flexbox;\r\n            display: -webkit-flex;\r\n            display: flex\r\n        }\r\n\r\n        .gb_6d .gb_1b {\r\n            display: table-cell;\r\n            height: 48px;\r\n            vertical-align: middle\r\n        }\r\n\r\n        .gb_6d .gb_tc, .gb_6d .gb_Ud {\r\n            background-color: #f5f5f5;\r\n            display: block\r\n        }\r\n\r\n            .gb_6d .gb_Ud .gb_Zc {\r\n                float: right\r\n            }\r\n\r\n        .gb_rc.gb_9b .gb_6d .gb_tc, .gb_rc.gb_9b .gb_6d .gb_Ud {\r\n            padding: 4px\r\n        }\r\n\r\n        .gb_rc:not(.gb_9b) .gb_6d .gb_tc, .gb_rc:not(.gb_9b) .gb_6d .gb_Ud {\r\n            padding: 8px\r\n        }\r\n\r\n        .gb_6d .gb_8a {\r\n            width: 40px\r\n        }\r\n\r\n        .gb_6d .gb_bb {\r\n            position: absolute;\r\n            right: 0;\r\n            top: 50%\r\n        }\r\n\r\n        .gb_7d {\r\n            position: relative\r\n        }\r\n\r\n        .gb_dc .gb_8d {\r\n            text-decoration: none\r\n        }\r\n\r\n        .gb_dc .gb_9d {\r\n            display: inline;\r\n            white-space: normal;\r\n            word-break: break-all;\r\n            word-break: break-word\r\n        }\r\n\r\n        body.gb_ae [data-ogpc] {\r\n            -webkit-transition: margin-left .25s cubic-bezier(0.4,0.0,0.2,1),visibility 0s linear .25s;\r\n            transition: margin-left .25s cubic-bezier(0.4,0.0,0.2,1),visibility 0s linear .25s\r\n        }\r\n\r\n        body.gb_ae.gb_be [data-ogpc] {\r\n            -webkit-transition: margin-left .25s cubic-bezier(0.4,0.0,0.2,1),visibility 0s linear 0s;\r\n            transition: margin-left .25s cubic-bezier(0.4,0.0,0.2,1),visibility 0s linear 0s\r\n        }\r\n\r\n        body [data-ogpc] {\r\n            margin-left: 0\r\n        }\r\n\r\n        body.gb_be [data-ogpc] {\r\n            margin-left: 280px\r\n        }\r\n\r\n        .gb_ce {\r\n            line-height: normal;\r\n            padding-right: 15px\r\n        }\r\n\r\n        .gb_P {\r\n            opacity: .75\r\n        }\r\n\r\n        a.gb_P, span.gb_P {\r\n            color: #000;\r\n            text-decoration: none\r\n        }\r\n\r\n        .gb_X a.gb_P, .gb_X span.gb_P {\r\n            color: #fff\r\n        }\r\n\r\n        a.gb_P:hover, a.gb_P:focus {\r\n            opacity: .85;\r\n            text-decoration: underline\r\n        }\r\n\r\n        .gb_Q {\r\n            display: inline-block;\r\n            padding-left: 15px\r\n        }\r\n\r\n            .gb_Q .gb_P {\r\n                display: inline-block;\r\n                line-height: 24px;\r\n                outline: none;\r\n                vertical-align: middle\r\n            }\r\n\r\n        .gb_S .gb_P {\r\n            display: none\r\n        }\r\n\r\n        .gb_fe {\r\n            padding-left: 16px\r\n        }\r\n\r\n            .gb_fe:not(.gb_9b) {\r\n                padding-left: 24px\r\n            }\r\n\r\n        .gb_ge {\r\n            color: black;\r\n            opacity: .54\r\n        }\r\n\r\n        .gb_he {\r\n            background: white;\r\n            -webkit-box-shadow: 0 5px 5px -3px rgba(0,0,0,0.2),0 8px 10px 1px rgba(0,0,0,0.14),0 3px 14px 2px rgba(0,0,0,0.12);\r\n            box-shadow: 0 5px 5px -3px rgba(0,0,0,0.2),0 8px 10px 1px rgba(0,0,0,0.14),0 3px 14px 2px rgba(0,0,0,0.12);\r\n            overflow-y: hidden;\r\n            position: absolute;\r\n            right: 24px;\r\n            top: 48px\r\n        }\r\n\r\n        .gb_Ua {\r\n            background-color: #f5f5f5;\r\n            display: inline-block;\r\n            overflow: hidden;\r\n            padding: 0;\r\n            vertical-align: middle;\r\n            -webkit-border-radius: 4px;\r\n            border-radius: 4px;\r\n            -webkit-box-shadow: 0 2px 1px -1px rgba(0,0,0,0.2),0 1px 1px 0 rgba(0,0,0,0.14),0 1px 3px 0 rgba(0,0,0,0.12);\r\n            box-shadow: 0 2px 1px -1px rgba(0,0,0,0.2),0 1px 1px 0 rgba(0,0,0,0.14),0 1px 3px 0 rgba(0,0,0,0.12)\r\n        }\r\n\r\n        .gb_Va {\r\n            background-color: #fff;\r\n            padding: 8px;\r\n            display: inline-block;\r\n            line-height: 32px;\r\n            text-align: center;\r\n            vertical-align: middle\r\n        }\r\n\r\n        .gb_Ua .gb_Wa.gb_Xa {\r\n            min-width: 0\r\n        }\r\n\r\n        .gb_7a {\r\n            -webkit-background-size: 32px 32px;\r\n            background-size: 32px 32px;\r\n            -webkit-border-radius: 50%;\r\n            border-radius: 50%;\r\n            display: block;\r\n            margin: 0;\r\n            overflow: hidden;\r\n            position: relative;\r\n            height: 32px;\r\n            width: 32px;\r\n            z-index: 0\r\n        }\r\n\r\n        @media (min-resolution:1.25dppx),(-o-min-device-pixel-ratio:5/4),(-webkit-min-device-pixel-ratio:1.25),(min-device-pixel-ratio:1.25) {\r\n            .gb_7a::before {\r\n                display: inline-block;\r\n                -webkit-transform: scale(.5);\r\n                transform: scale(.5);\r\n                -webkit-transform-origin: left 0;\r\n                transform-origin: left 0\r\n            }\r\n\r\n            .gb_ub::before {\r\n                display: inline-block;\r\n                -webkit-transform: scale(.5);\r\n                transform: scale(.5);\r\n                -webkit-transform-origin: left 0;\r\n                transform-origin: left 0\r\n            }\r\n        }\r\n\r\n        .gb_7a:hover, .gb_7a:focus {\r\n            -webkit-box-shadow: 0 1px 0 rgba(0,0,0,.15);\r\n            box-shadow: 0 1px 0 rgba(0,0,0,.15)\r\n        }\r\n\r\n        .gb_7a:active {\r\n            -webkit-box-shadow: inset 0 2px 0 rgba(0,0,0,.15);\r\n            box-shadow: inset 0 2px 0 rgba(0,0,0,.15)\r\n        }\r\n\r\n            .gb_7a:active::after {\r\n                background: rgba(0,0,0,.1);\r\n                -webkit-border-radius: 50%;\r\n                border-radius: 50%;\r\n                content: '';\r\n                display: block;\r\n                height: 100%\r\n            }\r\n\r\n        .gb_8a {\r\n            cursor: pointer;\r\n            line-height: 40px;\r\n            min-width: 30px;\r\n            opacity: .75;\r\n            overflow: hidden;\r\n            vertical-align: middle;\r\n            text-overflow: ellipsis\r\n        }\r\n\r\n        .gb_b.gb_8a {\r\n            width: auto\r\n        }\r\n\r\n        .gb_8a:hover, .gb_8a:focus {\r\n            opacity: .85\r\n        }\r\n\r\n        .gb_9a .gb_8a, .gb_9a .gb_ab {\r\n            line-height: 26px\r\n        }\r\n\r\n        #gb#gb.gb_9a a.gb_8a, .gb_9a .gb_ab {\r\n            font-size: 11px;\r\n            height: auto\r\n        }\r\n\r\n        .gb_bb {\r\n            border-top: 4px solid #000;\r\n            border-left: 4px dashed transparent;\r\n            border-right: 4px dashed transparent;\r\n            display: inline-block;\r\n            margin-left: 6px;\r\n            opacity: .75;\r\n            vertical-align: middle\r\n        }\r\n\r\n        .gb_cb:hover .gb_bb {\r\n            opacity: .85\r\n        }\r\n\r\n        .gb_X .gb_8a, .gb_X .gb_bb {\r\n            opacity: 1\r\n        }\r\n\r\n        #gb#gb.gb_X.gb_X a.gb_8a, #gb#gb .gb_X.gb_X a.gb_8a {\r\n            color: #fff\r\n        }\r\n\r\n        .gb_X.gb_X .gb_bb {\r\n            border-top-color: #fff;\r\n            opacity: 1\r\n        }\r\n\r\n        .gb_ea .gb_7a:hover, .gb_X .gb_7a:hover, .gb_ea .gb_7a:focus, .gb_X .gb_7a:focus {\r\n            -webkit-box-shadow: 0 1px 0 rgba(0,0,0,.15),0 1px 2px rgba(0,0,0,.2);\r\n            box-shadow: 0 1px 0 rgba(0,0,0,.15),0 1px 2px rgba(0,0,0,.2)\r\n        }\r\n\r\n        .gb_db .gb_eb, .gb_fb .gb_eb {\r\n            position: absolute;\r\n            right: 1px\r\n        }\r\n\r\n        .gb_eb.gb_R, .gb_gb.gb_R, .gb_cb.gb_R {\r\n            -webkit-flex: 0 1 auto;\r\n            flex: 0 1 auto;\r\n            -webkit-flex: 0 1 main-size;\r\n            flex: 0 1 main-size\r\n        }\r\n\r\n        .gb_hb.gb_W .gb_8a {\r\n            width: 30px !important\r\n        }\r\n\r\n        .gb_ib.gb_6a {\r\n            display: none\r\n        }\r\n\r\n        @-webkit-keyframes bar {\r\n            0% {\r\n                margin-left: -100%\r\n            }\r\n\r\n            to {\r\n                margin-left: 100%\r\n            }\r\n        }\r\n\r\n        @keyframes progressmove {\r\n            0% {\r\n                margin-left: -100%\r\n            }\r\n\r\n            to {\r\n                margin-left: 100%\r\n            }\r\n        }\r\n\r\n        .gb_jb.gb_5a {\r\n            display: none\r\n        }\r\n\r\n        .gb_jb {\r\n            background-color: #ccc;\r\n            height: 3px;\r\n            overflow: hidden\r\n        }\r\n\r\n        .gb_kb {\r\n            background-color: #f4b400;\r\n            height: 100%;\r\n            width: 50%;\r\n            -webkit-animation: progressmove 1.5s linear 0s infinite;\r\n            animation: progressmove 1.5s linear 0s infinite\r\n        }\r\n\r\n        .gb_eb .gb_b {\r\n            padding: 4px\r\n        }\r\n\r\n        .gb_je {\r\n            display: none\r\n        }\r\n\r\n        .gb_8b {\r\n            -webkit-border-radius: 50%;\r\n            border-radius: 50%;\r\n            display: inline-block;\r\n            margin: 0 4px;\r\n            padding: 12px;\r\n            overflow: hidden;\r\n            vertical-align: middle;\r\n            cursor: pointer;\r\n            height: 24px;\r\n            width: 24px;\r\n            -webkit-user-select: none;\r\n            -webkit-flex: 0 0 auto;\r\n            flex: 0 0 auto\r\n        }\r\n\r\n        .gb_9b .gb_8b {\r\n            margin: 0 4px 0 0\r\n        }\r\n\r\n        .gb_8b:focus, .gb_8b:hover {\r\n            background-color: rgba(0,0,0,0.071);\r\n            outline: none\r\n        }\r\n\r\n        .gb_ac {\r\n            display: none\r\n        }\r\n\r\n        .gb_bc {\r\n            -webkit-transform: none;\r\n            transform: none\r\n        }\r\n\r\n        .gb_cc {\r\n            display: none\r\n        }\r\n\r\n        .gb_dc {\r\n            background-color: #fff;\r\n            bottom: 0;\r\n            color: #000;\r\n            height: -webkit-calc(100vh - 100%);\r\n            height: calc(100vh - 100%);\r\n            overflow-y: auto;\r\n            overflow-x: hidden;\r\n            position: absolute;\r\n            top: 100%;\r\n            z-index: 990;\r\n            will-change: visibility;\r\n            visibility: hidden;\r\n            display: -webkit-flex;\r\n            display: flex;\r\n            -webkit-flex-direction: column;\r\n            flex-direction: column;\r\n            -webkit-transition: transform .25s cubic-bezier(0.4,0.0,0.2,1),visibility 0s linear .25s;\r\n            transition: transform .25s cubic-bezier(0.4,0.0,0.2,1),visibility 0s linear .25s\r\n        }\r\n\r\n            .gb_dc.gb_9b {\r\n                width: 264px;\r\n                -webkit-transform: translateX(-264px);\r\n                transform: translateX(-264px)\r\n            }\r\n\r\n            .gb_dc:not(.gb_9b) {\r\n                width: 280px;\r\n                -webkit-transform: translateX(-280px);\r\n                transform: translateX(-280px)\r\n            }\r\n\r\n            .gb_dc.gb_g {\r\n                -webkit-transform: translateX(0);\r\n                transform: translateX(0);\r\n                visibility: visible;\r\n                -webkit-box-shadow: 0 0 16px rgba(0,0,0,.28);\r\n                box-shadow: 0 0 16px rgba(0,0,0,.28);\r\n                -webkit-transition: transform .25s cubic-bezier(0.4,0.0,0.2,1),visibility 0s linear 0s;\r\n                transition: transform .25s cubic-bezier(0.4,0.0,0.2,1),visibility 0s linear 0s\r\n            }\r\n\r\n        .gb_ec.gb_fc {\r\n            background-color: transparent;\r\n            -webkit-box-shadow: 0 0;\r\n            box-shadow: 0 0\r\n        }\r\n\r\n            .gb_ec.gb_fc > :not(.gb_gc) {\r\n                display: none\r\n            }\r\n\r\n        .gb_gc {\r\n            display: -webkit-flex;\r\n            display: flex;\r\n            -webkit-flex: 1 1 auto;\r\n            flex: 1 1 auto;\r\n            -webkit-flex-direction: column;\r\n            flex-direction: column\r\n        }\r\n\r\n            .gb_gc > .gb_hc {\r\n                -webkit-flex: 1 0 auto;\r\n                flex: 1 0 auto\r\n            }\r\n\r\n            .gb_gc > .gb_ic {\r\n                -webkit-flex: 0 0 auto;\r\n                flex: 0 0 auto\r\n            }\r\n\r\n        .gb_jc {\r\n            list-style: none;\r\n            margin-top: 0;\r\n            margin-bottom: 0;\r\n            padding: 8px 0\r\n        }\r\n\r\n        .gb_dc:not(.gb_ec) .gb_jc:first-child {\r\n            padding: 0 0 8px 0\r\n        }\r\n\r\n        .gb_jc:not(:last-child) {\r\n            border-bottom: 1px solid #ddd\r\n        }\r\n\r\n        .gb_kc {\r\n            cursor: pointer\r\n        }\r\n\r\n        .gb_lc:empty {\r\n            display: none\r\n        }\r\n\r\n        .gb_kc, .gb_lc {\r\n            display: block;\r\n            min-height: 40px;\r\n            padding-bottom: 4px;\r\n            padding-top: 4px;\r\n            font-family: Roboto,RobotoDraft,Helvetica,Arial,sans-serif;\r\n            color: #767676\r\n        }\r\n\r\n        .gb_dc.gb_9b .gb_kc {\r\n            padding-left: 16px\r\n        }\r\n\r\n        .gb_dc:not(.gb_9b) .gb_kc, .gb_dc:not(.gb_9b) .gb_lc {\r\n            padding-left: 24px\r\n        }\r\n\r\n        .gb_kc:hover {\r\n            background-color: rgba(0,0,0,.03)\r\n        }\r\n\r\n        .gb_kc.gb_mc {\r\n            background: rgba(0,0,0,.05);\r\n            font-weight: bold;\r\n            color: #212121\r\n        }\r\n\r\n        .gb_kc .gb_nc {\r\n            text-decoration: none;\r\n            display: inline-block;\r\n            width: 100%\r\n        }\r\n\r\n            .gb_kc .gb_nc:focus {\r\n                outline: none\r\n            }\r\n\r\n        .gb_kc .gb_oc, .gb_lc {\r\n            padding-left: 32px;\r\n            display: inline-block;\r\n            line-height: 40px;\r\n            vertical-align: top;\r\n            width: 176px;\r\n            white-space: nowrap;\r\n            overflow: hidden;\r\n            text-overflow: ellipsis\r\n        }\r\n\r\n        .gb_gc.gb_5 .gb_nc:focus .gb_oc {\r\n            text-decoration: underline\r\n        }\r\n\r\n        .gb_kc .gb_pc {\r\n            height: 24px;\r\n            width: 24px;\r\n            float: left;\r\n            margin-top: 8px;\r\n            vertical-align: middle\r\n        }\r\n\r\n        .gb_Cc .gb_Fc {\r\n            font-size: 14px;\r\n            font-weight: bold;\r\n            top: 0;\r\n            right: 0\r\n        }\r\n\r\n        .gb_Cc .gb_b {\r\n            display: inline-block;\r\n            vertical-align: middle;\r\n            -webkit-box-sizing: border-box;\r\n            box-sizing: border-box;\r\n            height: 40px;\r\n            width: 40px\r\n        }\r\n\r\n        .gb_Cc .gb_lb {\r\n            border-bottom-color: #e5e5e5\r\n        }\r\n\r\n        .gb_Hc {\r\n            background-color: rgba(0,0,0,.55);\r\n            color: white;\r\n            font-size: 12px;\r\n            font-weight: bold;\r\n            line-height: 24px;\r\n            margin: 5px;\r\n            padding: 0 2px;\r\n            text-align: center;\r\n            -webkit-box-sizing: border-box;\r\n            box-sizing: border-box;\r\n            -webkit-border-radius: 50%;\r\n            border-radius: 50%;\r\n            height: 24px;\r\n            width: 24px\r\n        }\r\n\r\n            .gb_Hc.gb_Ic {\r\n                background-position: -79px 0\r\n            }\r\n\r\n            .gb_Hc.gb_Jc {\r\n                background-position: -79px -64px\r\n            }\r\n\r\n        .gb_b:hover .gb_Hc, .gb_b:focus .gb_Hc {\r\n            background-color: rgba(0,0,0,.85)\r\n        }\r\n\r\n        #gbsfw.gb_Kc {\r\n            background: #e5e5e5;\r\n            border-color: #ccc\r\n        }\r\n\r\n        .gb_ea .gb_Hc {\r\n            background-color: rgba(0,0,0,.7)\r\n        }\r\n\r\n        .gb_X .gb_Hc.gb_Hc, .gb_X .gb_zc .gb_Hc.gb_Hc, .gb_X .gb_zc .gb_b:hover .gb_Hc, .gb_X .gb_zc .gb_b:focus .gb_Hc {\r\n            background-color: #fff;\r\n            color: #404040\r\n        }\r\n\r\n        .gb_X .gb_Hc.gb_Ic {\r\n            background-position: -54px -64px\r\n        }\r\n\r\n        .gb_X .gb_Hc.gb_Jc {\r\n            background-position: 0 -64px\r\n        }\r\n\r\n        .gb_zc .gb_Hc.gb_Hc {\r\n            background-color: #db4437;\r\n            color: white\r\n        }\r\n\r\n        .gb_zc .gb_b:hover .gb_Hc, .gb_zc .gb_b:focus .gb_Hc {\r\n            background-color: #a52714\r\n        }\r\n\r\n        .gb_me {\r\n            line-height: 20px;\r\n            margin: 2px;\r\n            text-align: center;\r\n            vertical-align: middle;\r\n            -webkit-box-sizing: border-box;\r\n            box-sizing: border-box;\r\n            -webkit-border-radius: 50%;\r\n            border-radius: 50%;\r\n            height: 20px;\r\n            width: 20px\r\n        }\r\n\r\n        .gb_Cc a {\r\n            line-height: 24px;\r\n            -webkit-border-radius: 50%;\r\n            border-radius: 50%\r\n        }\r\n\r\n        .gb_ne.gb_Ic .gb_me {\r\n            display: none\r\n        }\r\n\r\n        .gb_ne svg {\r\n            display: none;\r\n            height: 24px;\r\n            width: 24px\r\n        }\r\n\r\n        .gb_ne.gb_Ic svg {\r\n            display: block\r\n        }\r\n\r\n        .gb_ne.gb_Jc svg {\r\n            display: none\r\n        }\r\n\r\n        .gb_zc .gb_me {\r\n            color: white;\r\n            background-color: #db4437;\r\n            opacity: 1\r\n        }\r\n\r\n        .gb_1d .gb_zc .gb_me {\r\n            color: white;\r\n            background-color: #db4437\r\n        }\r\n\r\n        .gb_2d .gb_zc .gb_me {\r\n            color: #212121;\r\n            background-color: white\r\n        }\r\n\r\n        .gb_oe {\r\n            display: none\r\n        }\r\n\r\n        .gb_hf {\r\n            cursor: pointer;\r\n            padding: 13px\r\n        }\r\n\r\n        .gb_if {\r\n            background-color: rgba(0,0,0,0.1);\r\n            -webkit-box-shadow: inset 1px 1px 3px rgba(0,0,0,.24);\r\n            box-shadow: inset 1px 1px 3px rgba(0,0,0,.24);\r\n            width: 34px;\r\n            height: 17px;\r\n            -webkit-border-radius: 8px;\r\n            border-radius: 8px;\r\n            position: relative;\r\n            -webkit-transition: background-color ease 150ms;\r\n            transition: background-color ease 150ms\r\n        }\r\n\r\n        .gb_hf[aria-pressed=true] .gb_if {\r\n            background-color: rgba(255,255,255,0.1)\r\n        }\r\n\r\n        .gb_jf {\r\n            position: absolute;\r\n            width: 25px;\r\n            height: 25px;\r\n            -webkit-border-radius: 50%;\r\n            border-radius: 50%;\r\n            -webkit-box-shadow: 0 0 2px rgba(0,0,0,.12),0 2px 4px rgba(0,0,0,.24);\r\n            box-shadow: 0 0 2px rgba(0,0,0,.12),0 2px 4px rgba(0,0,0,.24);\r\n            top: -4px;\r\n            -webkit-transform: translateX(-12px);\r\n            transform: translateX(-12px);\r\n            background-color: white;\r\n            -webkit-transition: transform ease 150ms;\r\n            transition: transform ease 150ms\r\n        }\r\n\r\n        .gb_hf[aria-pressed=true] .gb_jf {\r\n            -webkit-transform: translateX(20px);\r\n            transform: translateX(20px)\r\n        }\r\n\r\n        .gb_jf img {\r\n            position: absolute;\r\n            margin: 5px;\r\n            width: 15px;\r\n            height: 15px\r\n        }\r\n\r\n        .gb_Jd {\r\n            line-height: 0;\r\n            -webkit-user-select: none\r\n        }\r\n\r\n        .gb_Hd > .gb_Jd:only-child {\r\n            float: right\r\n        }\r\n\r\n        .gb_Jd .gb_re {\r\n            display: inline-block\r\n        }\r\n\r\n        .gb_Jd .gb_yb {\r\n            cursor: pointer\r\n        }\r\n\r\n            .gb_Jd .gb_yb img {\r\n                opacity: .54;\r\n                width: 24px;\r\n                height: 24px;\r\n                padding: 12px\r\n            }\r\n\r\n        .gb_2d .gb_Jd .gb_yb img {\r\n            opacity: 1\r\n        }\r\n\r\n        .gb_pe {\r\n            text-align: right\r\n        }\r\n\r\n        .gb_re {\r\n            text-align: initial\r\n        }\r\n\r\n        .gb_Jd .gb_se, .gb_Jd .gb_te {\r\n            display: table-cell;\r\n            height: 48px;\r\n            vertical-align: middle\r\n        }\r\n\r\n        .gb_Jd .gb_se {\r\n            overflow: hidden\r\n        }\r\n\r\n        .gb_Nc {\r\n            display: none\r\n        }\r\n\r\n            .gb_Nc.gb_g {\r\n                display: block\r\n            }\r\n\r\n        .gb_Oc {\r\n            background-color: #fff;\r\n            -webkit-box-shadow: 0 1px 0 rgba(0,0,0,0.08);\r\n            box-shadow: 0 1px 0 rgba(0,0,0,0.08);\r\n            color: #000;\r\n            position: relative;\r\n            z-index: 986\r\n        }\r\n\r\n        .gb_Pc {\r\n            height: 40px;\r\n            padding: 16px 24px;\r\n            white-space: nowrap\r\n        }\r\n\r\n        .gb_Qc {\r\n            position: fixed;\r\n            bottom: 16px;\r\n            padding: 16px;\r\n            right: 16px;\r\n            white-space: normal;\r\n            width: 328px;\r\n            -webkit-transition: width .2s,bottom .2s,right .2s;\r\n            transition: width .2s,bottom .2s,right .2s;\r\n            -webkit-border-radius: 2px;\r\n            border-radius: 2px;\r\n            -webkit-box-shadow: 0 5px 5px -3px rgba(0,0,0,0.2),0 8px 10px 1px rgba(0,0,0,0.14),0 3px 14px 2px rgba(0,0,0,0.12);\r\n            box-shadow: 0 5px 5px -3px rgba(0,0,0,0.2),0 8px 10px 1px rgba(0,0,0,0.14),0 3px 14px 2px rgba(0,0,0,0.12)\r\n        }\r\n\r\n        @media (max-width:400px) {\r\n            .gb_Oc.gb_Qc {\r\n                max-width: 368px;\r\n                width: auto;\r\n                bottom: 0;\r\n                right: 0\r\n            }\r\n        }\r\n\r\n        .gb_Oc .gb_yb {\r\n            border: 0;\r\n            font-weight: 500;\r\n            font-size: 14px;\r\n            line-height: 36px;\r\n            min-width: 32px;\r\n            padding: 0 16px;\r\n            vertical-align: middle\r\n        }\r\n\r\n            .gb_Oc .gb_yb:before {\r\n                content: '';\r\n                height: 6px;\r\n                left: 0;\r\n                position: absolute;\r\n                top: -6px;\r\n                width: 100%\r\n            }\r\n\r\n            .gb_Oc .gb_yb:after {\r\n                bottom: -6px;\r\n                content: '';\r\n                height: 6px;\r\n                left: 0;\r\n                position: absolute;\r\n                width: 100%\r\n            }\r\n\r\n            .gb_Oc .gb_yb + .gb_yb {\r\n                margin-left: 8px\r\n            }\r\n\r\n        .gb_Rc {\r\n            height: 48px;\r\n            padding: 4px;\r\n            margin: -8px 0 0 -8px\r\n        }\r\n\r\n        .gb_Qc .gb_Rc {\r\n            float: left;\r\n            margin: -4px\r\n        }\r\n\r\n        .gb_Sc {\r\n            font-family: Roboto,RobotoDraft,Helvetica,Arial,sans-serif;\r\n            overflow: hidden;\r\n            vertical-align: top\r\n        }\r\n\r\n        .gb_Pc .gb_Sc {\r\n            display: inline-block;\r\n            padding-left: 8px;\r\n            width: 640px\r\n        }\r\n\r\n        .gb_Qc .gb_Sc {\r\n            display: block;\r\n            margin-left: 56px;\r\n            padding-bottom: 16px\r\n        }\r\n\r\n        .gb_Tc {\r\n            background-color: inherit\r\n        }\r\n\r\n        .gb_Pc .gb_Tc {\r\n            display: inline-block;\r\n            position: absolute;\r\n            top: 18px;\r\n            right: 24px\r\n        }\r\n\r\n        .gb_Qc .gb_Tc {\r\n            text-align: right;\r\n            padding-right: 24px;\r\n            padding-top: 6px\r\n        }\r\n\r\n        .gb_Tc .gb_Uc {\r\n            height: 1.5em;\r\n            margin: -.25em 10px -.25em 0;\r\n            vertical-align: text-top;\r\n            width: 1.5em\r\n        }\r\n\r\n        .gb_Vc {\r\n            line-height: 20px;\r\n            font-size: 16px;\r\n            font-weight: 700;\r\n            color: rgba(0,0,0,.87)\r\n        }\r\n\r\n        .gb_Qc .gb_Vc {\r\n            color: rgba(0,0,0,.87);\r\n            font-size: 16px;\r\n            line-height: 20px;\r\n            padding-top: 8px\r\n        }\r\n\r\n        .gb_Pc .gb_Vc, .gb_Pc .gb_Wc {\r\n            width: 640px\r\n        }\r\n\r\n        .gb_Wc .gb_Xc, .gb_Wc {\r\n            line-height: 20px;\r\n            font-size: 13px;\r\n            font-weight: 400;\r\n            color: rgba(0,0,0,.54)\r\n        }\r\n\r\n        .gb_Qc .gb_Wc .gb_Xc {\r\n            font-size: 14px\r\n        }\r\n\r\n        .gb_Qc .gb_Wc {\r\n            padding-top: 12px\r\n        }\r\n\r\n            .gb_Qc .gb_Wc a {\r\n                color: rgba(66,133,244,1)\r\n            }\r\n\r\n        .gb_Zc.gb_0c {\r\n            padding: 0\r\n        }\r\n\r\n        .gb_0c .gb_fa {\r\n            padding: 26px 26px 22px 13px;\r\n            background: #ffffff\r\n        }\r\n\r\n        .gb_1c.gb_0c .gb_fa {\r\n            background: #4d90fe\r\n        }\r\n\r\n        a.gb_2c {\r\n            color: #666666 !important;\r\n            font-size: 22px;\r\n            height: 9px;\r\n            opacity: .8;\r\n            position: absolute;\r\n            right: 14px;\r\n            top: 4px;\r\n            text-decoration: none !important;\r\n            width: 9px\r\n        }\r\n\r\n        .gb_1c a.gb_2c {\r\n            color: #c1d1f4 !important\r\n        }\r\n\r\n        a.gb_2c:hover, a.gb_2c:active {\r\n            opacity: 1\r\n        }\r\n\r\n        .gb_3c {\r\n            padding: 0;\r\n            width: 258px;\r\n            white-space: normal;\r\n            display: table\r\n        }\r\n\r\n        .gb_4c .gb_fa {\r\n            top: 56px;\r\n            border: 0;\r\n            padding: 16px;\r\n            -webkit-box-shadow: 4px 4px 12px rgba(0,0,0,0.4);\r\n            box-shadow: 4px 4px 12px rgba(0,0,0,0.4)\r\n        }\r\n\r\n        .gb_4c .gb_3c {\r\n            width: 328px\r\n        }\r\n\r\n        .gb_4c .gb_Fa, .gb_4c .gb_5c, .gb_4c .gb_Xc, .gb_4c .gb_Ba, .gb_6c {\r\n            line-height: normal;\r\n            font-family: Roboto,RobotoDraft,Helvetica,Arial,sans-serif\r\n        }\r\n\r\n        .gb_4c .gb_Fa, .gb_4c .gb_5c, .gb_4c .gb_Ba {\r\n            font-weight: 500\r\n        }\r\n\r\n        .gb_4c .gb_Fa, .gb_4c .gb_Ba {\r\n            border: 0;\r\n            padding: 10px 8px\r\n        }\r\n\r\n        .gb_0c .gb_Fa:active {\r\n            outline: none;\r\n            -webkit-box-shadow: 0 4px 5px rgba(0,0,0,.16);\r\n            box-shadow: 0 4px 5px rgba(0,0,0,.16)\r\n        }\r\n\r\n        .gb_4c .gb_5c {\r\n            color: #222;\r\n            margin-bottom: 8px\r\n        }\r\n\r\n        .gb_4c .gb_Xc {\r\n            color: #808080;\r\n            font-size: 14px\r\n        }\r\n\r\n        .gb_7c {\r\n            text-align: right;\r\n            font-size: 14px;\r\n            padding-bottom: 0;\r\n            white-space: nowrap\r\n        }\r\n\r\n            .gb_7c .gb_8c {\r\n                margin-left: 8px\r\n            }\r\n\r\n            .gb_7c .gb_9c.gb_8c img {\r\n                background-color: inherit;\r\n                -webkit-border-radius: initial;\r\n                border-radius: initial;\r\n                height: 1.5em;\r\n                margin: -0.25em 10px -0.25em 2px;\r\n                vertical-align: text-top;\r\n                width: 1.5em\r\n            }\r\n\r\n        .gb_4c .gb_3c .gb_ad .gb_9c {\r\n            border: 2px solid transparent\r\n        }\r\n\r\n            .gb_4c .gb_3c .gb_ad .gb_9c:focus {\r\n                border-color: #bbccff\r\n            }\r\n\r\n                .gb_4c .gb_3c .gb_ad .gb_9c:focus:after, .gb_4c .gb_3c .gb_ad .gb_9c:hover:after {\r\n                    background-color: transparent\r\n                }\r\n\r\n        .gb_6c {\r\n            background-color: #404040;\r\n            color: #fff;\r\n            padding: 16px;\r\n            position: absolute;\r\n            top: 56px;\r\n            min-width: 328px;\r\n            max-width: 650px;\r\n            right: 8px;\r\n            -webkit-border-radius: 2px;\r\n            border-radius: 2px;\r\n            -webkit-box-shadow: 4px 4px 12px rgba(0,0,0,0.4);\r\n            box-shadow: 4px 4px 12px rgba(0,0,0,0.4)\r\n        }\r\n\r\n            .gb_6c a, .gb_6c a:visited {\r\n                color: #5e97f6;\r\n                text-decoration: none\r\n            }\r\n\r\n        .gb_bd {\r\n            text-transform: uppercase\r\n        }\r\n\r\n        .gb_cd {\r\n            padding-left: 50px\r\n        }\r\n\r\n        .gb_1c .gb_3c {\r\n            width: 200px\r\n        }\r\n\r\n        .gb_5c {\r\n            color: #333333;\r\n            font-size: 16px;\r\n            line-height: 20px;\r\n            margin: 0;\r\n            margin-bottom: 16px\r\n        }\r\n\r\n        .gb_1c .gb_5c {\r\n            color: #ffffff\r\n        }\r\n\r\n        .gb_Xc {\r\n            color: #666666;\r\n            line-height: 17px;\r\n            margin: 0;\r\n            margin-bottom: 5px\r\n        }\r\n\r\n        .gb_1c .gb_Xc {\r\n            color: #ffffff\r\n        }\r\n\r\n        .gb_dd {\r\n            text-decoration: none;\r\n            color: #5e97f6\r\n        }\r\n\r\n            .gb_dd:visited {\r\n                color: #5e97f6\r\n            }\r\n\r\n            .gb_dd:hover, .gb_dd:active {\r\n                text-decoration: underline\r\n            }\r\n\r\n        .gb_ed {\r\n            position: absolute;\r\n            background: transparent;\r\n            top: -999px;\r\n            z-index: -1;\r\n            visibility: hidden;\r\n            margin-top: 1px;\r\n            margin-left: 1px\r\n        }\r\n\r\n        #gb .gb_0c {\r\n            margin: 0\r\n        }\r\n\r\n        .gb_0c .gb_yb {\r\n            background: #4d90fe;\r\n            border-color: #3079ed;\r\n            margin-top: 15px\r\n        }\r\n\r\n        .gb_4c .gb_Fa {\r\n            background: #4285f4\r\n        }\r\n\r\n        #gb .gb_0c a.gb_yb.gb_yb {\r\n            color: #ffffff\r\n        }\r\n\r\n        .gb_0c .gb_yb:hover {\r\n            background: #357ae8;\r\n            border-color: #2f5bb7\r\n        }\r\n\r\n        .gb_fd .gb_Fc .gb_lb {\r\n            border-bottom-color: #ffffff;\r\n            display: block\r\n        }\r\n\r\n        .gb_gd .gb_Fc .gb_lb {\r\n            border-bottom-color: #4d90fe;\r\n            display: block\r\n        }\r\n\r\n        .gb_fd .gb_Fc .gb_mb, .gb_gd .gb_Fc .gb_mb {\r\n            display: block\r\n        }\r\n\r\n        .gb_hd, .gb_ad {\r\n            display: table-cell\r\n        }\r\n\r\n        .gb_hd {\r\n            vertical-align: middle\r\n        }\r\n\r\n        .gb_4c .gb_hd {\r\n            vertical-align: top\r\n        }\r\n\r\n        .gb_ad {\r\n            padding-left: 13px;\r\n            width: 100%\r\n        }\r\n\r\n        .gb_4c .gb_ad {\r\n            padding-left: 20px\r\n        }\r\n\r\n        .gb_id {\r\n            display: block;\r\n            display: inline-block;\r\n            padding: 1em 0 0 0;\r\n            position: relative;\r\n            width: 100%\r\n        }\r\n\r\n        .gb_jd {\r\n            color: #ff0000;\r\n            font-style: italic;\r\n            margin: 0;\r\n            padding-left: 46px\r\n        }\r\n\r\n        .gb_id .gb_kd {\r\n            float: right;\r\n            margin: -20px 0;\r\n            width: -webkit-calc(100% - 46px);\r\n            width: calc(100% - 46px)\r\n        }\r\n\r\n        .gb_ld svg {\r\n            fill: grey\r\n        }\r\n\r\n        .gb_ld.gb_md svg {\r\n            fill: #4285f4\r\n        }\r\n\r\n        .gb_id .gb_kd label:after {\r\n            background-color: #4285f4\r\n        }\r\n\r\n        .gb_ld {\r\n            display: inline;\r\n            float: right;\r\n            margin-right: 22px;\r\n            position: relative;\r\n            top: -4px\r\n        }\r\n\r\n        .gb_nd {\r\n            color: #ffffff;\r\n            font-size: 13px;\r\n            font-weight: bold;\r\n            height: 25px;\r\n            line-height: 19px;\r\n            padding-top: 5px;\r\n            padding-left: 12px;\r\n            position: relative;\r\n            background-color: #4d90fe\r\n        }\r\n\r\n            .gb_nd .gb_od {\r\n                color: #ffffff;\r\n                cursor: default;\r\n                font-size: 22px;\r\n                font-weight: normal;\r\n                position: absolute;\r\n                right: 12px;\r\n                top: 5px\r\n            }\r\n\r\n            .gb_nd .gb_8c, .gb_nd .gb_pd {\r\n                color: #ffffff;\r\n                display: inline-block;\r\n                font-size: 11px;\r\n                margin-left: 16px;\r\n                padding: 0 8px;\r\n                white-space: nowrap\r\n            }\r\n\r\n        .gb_qd {\r\n            background: none;\r\n            background-image: -webkit-gradient(linear,left top,left bottom,from(rgba(0,0,0,0.16)),to(rgba(0,0,0,0.2)));\r\n            background-image: -webkit-linear-gradient(top,rgba(0,0,0,0.16),rgba(0,0,0,0.2));\r\n            background-image: linear-gradient(top,rgba(0,0,0,0.16),rgba(0,0,0,0.2));\r\n            background-image: -webkit-linear-gradient(top,rgba(0,0,0,0.16),rgba(0,0,0,0.2));\r\n            border-radius: 2px;\r\n            border: 1px solid #dcdcdc;\r\n            border: 1px solid rgba(0,0,0,0.1);\r\n            cursor: default !important;\r\n            filter: progid:DXImageTransform.Microsoft.gradient(startColorstr=#160000ff,endColorstr=#220000ff);\r\n            text-decoration: none !important;\r\n            -webkit-border-radius: 2px\r\n        }\r\n\r\n            .gb_qd:hover {\r\n                background: none;\r\n                background-image: -webkit-gradient(linear,left top,left bottom,from(rgba(0,0,0,0.14)),to(rgba(0,0,0,0.2)));\r\n                background-image: -webkit-linear-gradient(top,rgba(0,0,0,0.14),rgba(0,0,0,0.2));\r\n                background-image: linear-gradient(top,rgba(0,0,0,0.14),rgba(0,0,0,0.2));\r\n                background-image: -webkit-linear-gradient(top,rgba(0,0,0,0.14),rgba(0,0,0,0.2));\r\n                border: 1px solid rgba(0,0,0,0.2);\r\n                box-shadow: 0 1px 1px rgba(0,0,0,0.1);\r\n                -webkit-box-shadow: 0 1px 1px rgba(0,0,0,0.1);\r\n                filter: progid:DXImageTransform.Microsoft.gradient(startColorstr=#14000000,endColorstr=#22000000)\r\n            }\r\n\r\n            .gb_qd:active {\r\n                box-shadow: inset 0 1px 2px rgba(0,0,0,0.3);\r\n                -webkit-box-shadow: inset 0 1px 2px rgba(0,0,0,0.3)\r\n            }\r\n\r\n        .gb_rc .gb_Ba {\r\n            color: #4285f4\r\n        }\r\n\r\n        .gb_rc .gb_Ca {\r\n            color: #fff\r\n        }\r\n\r\n        .gb_rc .gb_yb:not(.gb_ue):focus {\r\n            outline: none\r\n        }\r\n\r\n        .gb_We, .gb_Xe, .gb_Ze {\r\n            display: none\r\n        }\r\n\r\n        .gb_Dd {\r\n            height: 48px;\r\n            max-width: 720px\r\n        }\r\n\r\n        .gb_Hd.gb_Sd .gb_Dd {\r\n            max-width: 100%;\r\n            -webkit-flex: 1 1 auto;\r\n            flex: 1 1 auto\r\n        }\r\n\r\n        .gb_Ad > .gb_sc .gb_Dd {\r\n            display: table-cell;\r\n            vertical-align: middle;\r\n            width: 100%\r\n        }\r\n\r\n        .gb_Hd.gb_Sd .gb_Dd .gb_xe {\r\n            margin-left: 0;\r\n            margin-right: 0\r\n        }\r\n\r\n        .gb_xe {\r\n            background: rgba(0,0,0,0.04);\r\n            border: 1px solid rgba(0,0,0,0);\r\n            -webkit-border-radius: 4px;\r\n            border-radius: 4px;\r\n            margin-left: auto;\r\n            margin-right: auto;\r\n            max-width: 720px;\r\n            position: relative;\r\n            -webkit-transition: background 100ms ease-in,width 100ms ease-out;\r\n            transition: background 100ms ease-in,width 100ms ease-out\r\n        }\r\n\r\n            .gb_xe.gb_0e {\r\n                -webkit-border-radius: 4px 4px 0 0;\r\n                border-radius: 4px 4px 0 0\r\n            }\r\n\r\n        .gb_2d .gb_xe {\r\n            background: rgba(255,255,255,0.16)\r\n        }\r\n\r\n        .gb_1e.gb_xe {\r\n            background: rgba(255,255,255,1)\r\n        }\r\n\r\n        .gb_2d .gb_1e.gb_xe .gb_Pe {\r\n            color: rgba(0,0,0,0.87)\r\n        }\r\n\r\n        .gb_xe button {\r\n            background: none;\r\n            border: none;\r\n            cursor: pointer;\r\n            outline: none;\r\n            padding: 0 4px;\r\n            line-height: 0\r\n        }\r\n\r\n            .gb_xe button svg, .gb_xe button img {\r\n                padding: 7px;\r\n                margin: 4px\r\n            }\r\n\r\n        .gb_Re {\r\n            float: left\r\n        }\r\n\r\n        .gb_Qe .gb_Re {\r\n            position: absolute;\r\n            right: 0\r\n        }\r\n\r\n        .gb_2e {\r\n            display: none;\r\n            float: left\r\n        }\r\n\r\n        .gb_3e {\r\n            position: absolute;\r\n            right: 0;\r\n            cursor: default;\r\n            visibility: hidden;\r\n            top: 0;\r\n            -webkit-transition: opacity 250ms ease-out;\r\n            transition: opacity 250ms ease-out\r\n        }\r\n\r\n        .gb_4e .gb_3e {\r\n            right: 35px\r\n        }\r\n\r\n        .gb_Qe .gb_3e {\r\n            display: none\r\n        }\r\n\r\n        .gb_3e.gb_5e {\r\n            visibility: inherit\r\n        }\r\n\r\n        .gb_Pe::-ms-clear {\r\n            display: none;\r\n            height: 0;\r\n            width: 0\r\n        }\r\n\r\n        .gb_6e {\r\n            position: absolute;\r\n            right: 0;\r\n            top: 0\r\n        }\r\n\r\n        .gb_Qe .gb_6e {\r\n            right: 35px\r\n        }\r\n\r\n        .gb_Qe > .gb_7e {\r\n            padding: 0 11px\r\n        }\r\n\r\n        .gb_7e {\r\n            height: 46px;\r\n            padding: 0;\r\n            margin-right: 50px;\r\n            overflow: hidden\r\n        }\r\n\r\n        .gb_4e .gb_7e {\r\n            margin-right: 83px\r\n        }\r\n\r\n        .gb_Pe {\r\n            border: none;\r\n            font: normal 16px Roboto,sans-serif;\r\n            -webkit-font-variant-ligatures: none;\r\n            font-variant-ligatures: none;\r\n            height: 46px;\r\n            outline: none;\r\n            padding: 11px 16px 11px 16px;\r\n            width: 100%;\r\n            background: transparent;\r\n            -webkit-box-sizing: border-box;\r\n            box-sizing: border-box\r\n        }\r\n\r\n        .gb_2d .gb_Pe {\r\n            color: rgba(255,255,255,0.7)\r\n        }\r\n\r\n        .gb_Pe.gb_8e {\r\n            padding-left: 0\r\n        }\r\n\r\n        .gb_8e {\r\n            height: 46px;\r\n            line-height: 46px;\r\n            padding-bottom: 0;\r\n            padding-top: 0\r\n        }\r\n\r\n        .gb_xe:not(.gb_Oe) input::-webkit-input-placeholder {\r\n            color: rgba(0,0,0,0.54)\r\n        }\r\n\r\n        .gb_2d .gb_xe:not(.gb_Oe) input::-webkit-input-placeholder {\r\n            color: rgba(255,255,255,0.7)\r\n        }\r\n\r\n        .gb_xe.gb_Cd[aria-expanded=\"false\"] {\r\n            background: transparent;\r\n            float: right\r\n        }\r\n\r\n            .gb_xe.gb_Cd[aria-expanded=\"false\"] .gb_7e, .gb_xe.gb_Cd[aria-expanded=\"false\"] .gb_3e, .gb_xe.gb_Cd[aria-expanded=\"false\"] .gb_6e {\r\n                display: none\r\n            }\r\n\r\n        .gb_xe.gb_Cd[aria-expanded=\"true\"] {\r\n            margin-left: 0;\r\n            position: absolute;\r\n            width: auto\r\n        }\r\n\r\n            .gb_xe.gb_Cd[aria-expanded=\"true\"]:not(.gb_Qe) .gb_Re {\r\n                display: none\r\n            }\r\n\r\n        .gb_xe.gb_Cd .gb_Re {\r\n            padding: 0\r\n        }\r\n\r\n        .gb_xe.gb_Cd[aria-expanded=\"true\"] .gb_2e {\r\n            display: block\r\n        }\r\n\r\n        .gb_9e {\r\n            position: relative\r\n        }\r\n\r\n        .gb_af {\r\n            margin: 0 56px;\r\n            padding: 0;\r\n            text-align: center;\r\n            white-space: nowrap;\r\n            -webkit-user-select: none;\r\n            overflow: auto\r\n        }\r\n\r\n            .gb_af::-webkit-scrollbar {\r\n                display: none\r\n            }\r\n\r\n        .gb_9b .gb_af, .gb_td .gb_af {\r\n            margin: 0\r\n        }\r\n\r\n        .gb_bf, .gb_cf {\r\n            margin: 12px 0;\r\n            position: absolute;\r\n            top: 0;\r\n            height: 24px;\r\n            width: 24px\r\n        }\r\n\r\n        .gb_bf {\r\n            left: 0;\r\n            margin-left: 32px\r\n        }\r\n\r\n        .gb_cf {\r\n            margin-right: 32px;\r\n            right: 0\r\n        }\r\n\r\n        .gb_9b .gb_bf, .gb_9b .gb_cf {\r\n            margin-left: 0;\r\n            margin-right: 0\r\n        }\r\n\r\n        .gb_bf, .gb_cf {\r\n            visibility: hidden\r\n        }\r\n\r\n        .gb_9e.gb_df:hover .gb_bf, .gb_9e.gb_ef:hover .gb_cf {\r\n            visibility: visible\r\n        }\r\n\r\n        .gb_9e.gb_df .gb_af {\r\n            -webkit-mask-image: -webkit-linear-gradient(left,rgba(0,0,0,0),rgba(0,0,0,1) 100px)\r\n        }\r\n\r\n        .gb_9e.gb_ef .gb_af {\r\n            -webkit-mask-image: -webkit-linear-gradient(right,rgba(0,0,0,0),rgba(0,0,0,1) 100px)\r\n        }\r\n\r\n        .gb_9e.gb_df.gb_ef .gb_af {\r\n            -webkit-mask-image: -webkit-linear-gradient(left,rgba(0,0,0,0),rgba(0,0,0,1) 100px,rgba(0,0,0,1) 50%,rgba(0,0,0,0) 50%),-webkit-linear-gradient(right,rgba(0,0,0,0),rgba(0,0,0,1) 100px,rgba(0,0,0,1) 50%,rgba(0,0,0,0) 50%)\r\n        }\r\n\r\n        .gb_9b .gb_9e.gb_df .gb_af {\r\n            -webkit-mask-image: -webkit-linear-gradient(left,rgba(0,0,0,0),rgba(0,0,0,1) 50px)\r\n        }\r\n\r\n        .gb_9b .gb_9e.gb_ef .gb_af {\r\n            -webkit-mask-image: -webkit-linear-gradient(right,rgba(0,0,0,0),rgba(0,0,0,1) 50px)\r\n        }\r\n\r\n        .gb_9b .gb_9e.gb_df.gb_ef .gb_af {\r\n            -webkit-mask-image: -webkit-linear-gradient(left,rgba(0,0,0,0),rgba(0,0,0,1) 50px,rgba(0,0,0,1) 50%,rgba(0,0,0,0) 50%),-webkit-linear-gradient(right,rgba(0,0,0,0),rgba(0,0,0,1) 50px,rgba(0,0,0,1) 50%,rgba(0,0,0,0) 50%)\r\n        }\r\n\r\n        .gb_ff {\r\n            cursor: pointer;\r\n            display: inline-table;\r\n            outline: none\r\n        }\r\n\r\n            .gb_ff > .gb_gf {\r\n                border: 0 solid transparent;\r\n                border-width: 2px 0;\r\n                display: table-cell;\r\n                height: 44px;\r\n                padding: 0 24px;\r\n                opacity: .7;\r\n                text-decoration: none;\r\n                text-transform: uppercase;\r\n                vertical-align: middle\r\n            }\r\n\r\n            .gb_ff.gb_mc:focus {\r\n                background-color: rgba(0,0,0,.16)\r\n            }\r\n\r\n            .gb_ff.gb_mc > .gb_gf {\r\n                border-bottom-color: black;\r\n                opacity: 1\r\n            }\r\n\r\n        .gb_2d .gb_ff.gb_mc > .gb_gf {\r\n            border-bottom-color: white\r\n        }\r\n\r\n        .gb_1d .gb_ff.gb_mc > .gb_gf {\r\n            border-bottom-color: black\r\n        }\r\n\r\n        sentinel {\r\n        }\r\n\r\n        .gbii::before {\r\n            content: url(https://lh3.googleusercontent.com/-BklMYgK5aTM/AAAAAAAAAAI/AAAAAAAAAAA/ACnBePbd6r1V3ADdke3GG1wEjiAFoznpjg/s32-c-mo/photo.jpg)\r\n        }\r\n\r\n        .gbip::before {\r\n            content: url(https://lh3.googleusercontent.com/-BklMYgK5aTM/AAAAAAAAAAI/AAAAAAAAAAA/ACnBePbd6r1V3ADdke3GG1wEjiAFoznpjg/s96-c-mo/photo.jpg)\r\n        }\r\n\r\n        @media (min-resolution:1.25dppx),(-o-min-device-pixel-ratio:5/4),(-webkit-min-device-pixel-ratio:1.25),(min-device-pixel-ratio:1.25) {\r\n            .gbii::before {\r\n                content: url(https://lh3.googleusercontent.com/-BklMYgK5aTM/AAAAAAAAAAI/AAAAAAAAAAA/ACnBePbd6r1V3ADdke3GG1wEjiAFoznpjg/s64-c-mo/photo.jpg)\r\n            }\r\n\r\n            .gbip::before {\r\n                content: url(https://lh3.googleusercontent.com/-BklMYgK5aTM/AAAAAAAAAAI/AAAAAAAAAAA/ACnBePbd6r1V3ADdke3GG1wEjiAFoznpjg/s192-c-mo/photo.jpg)\r\n            }\r\n        }\r\n    </style>\r\n    <script>\r\n        ; this.gbar_ = { CONFIG: [[[0, \"www.gstatic.com\", \"og.qtm.en_US.OACggR9yFxc.O\", \"ch\", \"de\", \"330\", 0, [4, 2, \".40.40.\", \"\", \"1300102,3700267,3700436,3700476\", \"1505961578\", \"0\"], null, \"IM_OWZniCoyDmQGex5z4CA\", null, 0, \"og.qtm.-ik32g7dsb7yz.L.W.O\", \"AA2YrTu0k-Qu91KJraKiFXAN8rHeJqH8_g\", \"AA2YrTs-hGbIZcWAFiAYF7j35GzF-WERww\", \"\", 2, 0, 200, \"CHE\"], null, null, null, [1, 1, 0, null, \"0\", \"mathias.minder@gmail.com\", \"\", \"ADQF8tpQ4FJfKGGb1HoCC_HB4e6-TOvjMP9YBfKszlUPumc01DRb3ZWE5qGdCkR0e8zsk2xRa2ZZPdcgfmE6j95wr8AA-wd2-Q\"], [0, 0, \"\", 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0], [\"%1$s (Standard)\", \"Markenkonto\", 1, \"%1$s (delegiert)\", 1, null, 96, \"/forms/d/e/1FAIpQLSe2uZzZU0xKyIOCXPfo6kdklnDnrQf9qipNMaVQAaI-vW7h2w/viewform?authuser=$authuser\", null, null, null, 1, \"https://accounts.google.com/ListAccounts?authuser=0\\u0026pid=330\\u0026mo=1\\u0026mn=1\\u0026hl=de\", 0, \"dashboard\", null, null, null, null, \"Profil\", \"\", 1, 1, \"Abgemeldet\", \"https://accounts.google.com/AccountChooser?source=ogb\\u0026continue=$continue\\u0026Email=$email\", \"https://accounts.google.com/RemoveLocalAccount?source=ogb\\u0026Email=$email\", \"ENTFERNEN\", \"ANMELDEN\", 0, 0, 1, 0, 0], null, [\"1\", \"gci_91f30755d6a6b787dcc2a4062e6e9824.js\", \"googleapis.client:plusone:gapi.iframes\", \"0\", \"de\"], null, null, null, [1, null, null, \"[[]]\", [\"https\", \"ogs.google.com\", 0, \"/u/0\", \"rt=j\\u0026sourceid=330\", [\"/u/0/_/og/customization/get\", \"\"], [\"/u/0/_/og/customization/set\", \"\"], [\"/u/0/_/og/customization/remove\", \"\"]], \"ADQF8tpQ4FJfKGGb1HoCC_HB4e6-TOvjMP9YBfKszlUPumc01DRb3ZWE5qGdCkR0e8zsk2xRa2ZZPdcgfmE6j95wr8AA-wd2-Q\"], [\"m;/_/scs/abc-static/_/js/k=gapi.gapi.en.ZPSwvoEq44A.O/m=__features__/am=AAg/rt=j/d=1/rs=AHpOoo8-JL5R4cxPdwFdZ0Yu3_ek27rKCQ\", \"https://apis.google.com\", \"\", \"1\", \"1\", \"\", null, 1, \"es_plusone_gc_20170907.3_p0\", \"de\"], null, [0, 9.999999747378752e-05, 1, 40400, 330, \"CHE\", \"de\", \"1505961578.0\", 8], [[null, null, null, \"https://www.gstatic.com/og/_/js/k=og.qtm.en_US.OACggR9yFxc.O/rt=j/m=qgl,q_d,qdid,qmd,qmutsd/exm=qaaw,qabr,qadd,qaid,qalo,qano,qebr,qein,qhaw,qhbr,qhch,qhga,qhid,qhin,qhlo,qhmn,qhno,qhpc,qhpr,qhsf,qhtb,qhtt/d=1/ed=1/rs=AA2YrTu0k-Qu91KJraKiFXAN8rHeJqH8_g\"], [null, null, null, \"https://www.gstatic.com/og/_/ss/k=og.qtm.-ik32g7dsb7yz.L.W.O/m=q_d,qdid,qmd/excm=qaaw,qabr,qadd,qaid,qalo,qano,qebr,qein,qhaw,qhbr,qhch,qhga,qhid,qhin,qhlo,qhmn,qhno,qhpc,qhpr,qhsf,qhtb,qhtt/d=1/ed=1/rs=AA2YrTs-hGbIZcWAFiAYF7j35GzF-WERww\"]], null, null, [\"\"]]], };/* _GlobalPrefix_ */\r\n        this.gbar_ = this.gbar_ || {}; (function (_) {\r\n            var window = this;\r\n            /* _Module_:qhin */\r\n            try {\r\n                var aa, ba, ca, ra, sa; aa = \"function\" == typeof Object.defineProperties ? Object.defineProperty : function (a, c, d) { a != Array.prototype && a != Object.prototype && (a[c] = d.value) }; ba = \"undefined\" != typeof window && window === this ? this : \"undefined\" != typeof window.global && null != window.global ? window.global : this; ca = function (a, c) { if (c) { var d = ba; a = a.split(\".\"); for (var e = 0; e < a.length - 1; e++) { var f = a[e]; f in d || (d[f] = {}); d = d[f] } a = a[a.length - 1]; e = d[a]; c = c(e); c != e && null != c && aa(d, a, { configurable: !0, writable: !0, value: c }) } };\r\n                ca(\"String.prototype.startsWith\", function (a) { return a ? a : function (a, d) { if (null == this) throw new TypeError(\"The 'this' value for String.prototype.startsWith must not be null or undefined\"); if (a instanceof RegExp) throw new TypeError(\"First argument to String.prototype.startsWith must not be a regular expression\"); var c = this + \"\"; a += \"\"; var f = c.length, g = a.length; d = Math.max(0, Math.min(d | 0, c.length)); for (var h = 0; h < g && d < f;)if (c[d++] != a[h++]) return !1; return h >= g } }); ca(\"Number.MAX_SAFE_INTEGER\", function () { return 9007199254740991 });\r\n                _.da = _.da || {}; _.m = this; _.ea = function (a) { return void 0 !== a }; _.n = function (a) { return \"string\" == typeof a }; _.fa = function (a) { return \"number\" == typeof a }; _.ha = function (a) { a = a.split(\".\"); for (var c = _.m, d = 0; d < a.length; d++)if (c = c[a[d]], null == c) return null; return c }; _.ia = function () { }; _.ja = function (a) { a.ne = void 0; a.ra = function () { return a.ne ? a.ne : a.ne = new a } };\r\n                _.ka = function (a) {\r\n                    var c = typeof a; if (\"object\" == c) if (a) { if (a instanceof Array) return \"array\"; if (a instanceof Object) return c; var d = Object.prototype.toString.call(a); if (\"[object Window]\" == d) return \"object\"; if (\"[object Array]\" == d || \"number\" == typeof a.length && \"undefined\" != typeof a.splice && \"undefined\" != typeof a.propertyIsEnumerable && !a.propertyIsEnumerable(\"splice\")) return \"array\"; if (\"[object Function]\" == d || \"undefined\" != typeof a.call && \"undefined\" != typeof a.propertyIsEnumerable && !a.propertyIsEnumerable(\"call\")) return \"function\" } else return \"null\";\r\n                    else if (\"function\" == c && \"undefined\" == typeof a.call) return \"object\"; return c\r\n                }; _.la = function (a) { return \"array\" == _.ka(a) }; _.ma = function (a) { return \"function\" == _.ka(a) }; _.na = function (a) { var c = typeof a; return \"object\" == c && null != a || \"function\" == c }; _.oa = \"closure_uid_\" + (1E9 * Math.random() >>> 0); ra = function (a, c, d) { return a.call.apply(a.bind, arguments) };\r\n                sa = function (a, c, d) { if (!a) throw Error(); if (2 < arguments.length) { var e = Array.prototype.slice.call(arguments, 2); return function () { var d = Array.prototype.slice.call(arguments); Array.prototype.unshift.apply(d, e); return a.apply(c, d) } } return function () { return a.apply(c, arguments) } }; _.p = function (a, c, d) { Function.prototype.bind && -1 != Function.prototype.bind.toString().indexOf(\"native code\") ? _.p = ra : _.p = sa; return _.p.apply(null, arguments) }; _.ta = Date.now || function () { return +new Date };\r\n                _.t = function (a, c) { a = a.split(\".\"); var d = _.m; a[0] in d || !d.execScript || d.execScript(\"var \" + a[0]); for (var e; a.length && (e = a.shift());)!a.length && _.ea(c) ? d[e] = c : d[e] && d[e] !== Object.prototype[e] ? d = d[e] : d = d[e] = {} }; _.u = function (a, c) { function d() { } d.prototype = c.prototype; a.H = c.prototype; a.prototype = new d; a.prototype.constructor = a; a.Nj = function (a, d, g) { for (var e = Array(arguments.length - 2), f = 2; f < arguments.length; f++)e[f - 2] = arguments[f]; return c.prototype[d].apply(a, e) } };\r\n                _.va = function (a) { if (Error.captureStackTrace) Error.captureStackTrace(this, _.va); else { var c = Error().stack; c && (this.stack = c) } a && (this.message = String(a)) }; _.u(_.va, Error); _.va.prototype.name = \"CustomError\"; var za; _.wa = String.prototype.trim ? function (a) { return a.trim() } : function (a) { return a.replace(/^[\\s\\xa0]+|[\\s\\xa0]+$/g, \"\") };\r\n                _.Aa = function (a, c) { var d = 0; a = (0, _.wa)(String(a)).split(\".\"); c = (0, _.wa)(String(c)).split(\".\"); for (var e = Math.max(a.length, c.length), f = 0; 0 == d && f < e; f++) { var g = a[f] || \"\", h = c[f] || \"\"; do { g = /(\\d*)(\\D*)(.*)/.exec(g) || [\"\", \"\", \"\", \"\"]; h = /(\\d*)(\\D*)(.*)/.exec(h) || [\"\", \"\", \"\", \"\"]; if (0 == g[0].length && 0 == h[0].length) break; d = za(0 == g[1].length ? 0 : (0, window.parseInt)(g[1], 10), 0 == h[1].length ? 0 : (0, window.parseInt)(h[1], 10)) || za(0 == g[2].length, 0 == h[2].length) || za(g[2], h[2]); g = g[3]; h = h[3] } while (0 == d) } return d }; za = function (a, c) { return a < c ? -1 : a > c ? 1 : 0 };\r\n                _.Ba = Array.prototype.indexOf ? function (a, c, d) { return Array.prototype.indexOf.call(a, c, d) } : function (a, c, d) { d = null == d ? 0 : 0 > d ? Math.max(0, a.length + d) : d; if (_.n(a)) return _.n(c) && 1 == c.length ? a.indexOf(c, d) : -1; for (; d < a.length; d++)if (d in a && a[d] === c) return d; return -1 }; _.Ca = Array.prototype.forEach ? function (a, c, d) { Array.prototype.forEach.call(a, c, d) } : function (a, c, d) { for (var e = a.length, f = _.n(a) ? a.split(\"\") : a, g = 0; g < e; g++)g in f && c.call(d, f[g], g, a) };\r\n                _.Da = Array.prototype.filter ? function (a, c, d) { return Array.prototype.filter.call(a, c, d) } : function (a, c, d) { for (var e = a.length, f = [], g = 0, h = _.n(a) ? a.split(\"\") : a, l = 0; l < e; l++)if (l in h) { var q = h[l]; c.call(d, q, l, a) && (f[g++] = q) } return f }; _.Ea = Array.prototype.map ? function (a, c, d) { return Array.prototype.map.call(a, c, d) } : function (a, c, d) { for (var e = a.length, f = Array(e), g = _.n(a) ? a.split(\"\") : a, h = 0; h < e; h++)h in g && (f[h] = c.call(d, g[h], h, a)); return f }; _.Fa = Array.prototype.some ? function (a, c, d) { return Array.prototype.some.call(a, c, d) } : function (a, c, d) { for (var e = a.length, f = _.n(a) ? a.split(\"\") : a, g = 0; g < e; g++)if (g in f && c.call(d, f[g], g, a)) return !0; return !1 }; _.Ga = function (a, c) { return 0 <= (0, _.Ba)(a, c) };\r\n                a: { var Ia = _.m.navigator; if (Ia) { var Ja = Ia.userAgent; if (Ja) { _.Ha = Ja; break a } } _.Ha = \"\" } _.v = function (a) { return -1 != _.Ha.indexOf(a) }; var La; _.Ka = function (a, c, d) { for (var e in a) c.call(d, a[e], e, a) }; La = \"constructor hasOwnProperty isPrototypeOf propertyIsEnumerable toLocaleString toString valueOf\".split(\" \"); _.Ma = function (a, c) { for (var d, e, f = 1; f < arguments.length; f++) { e = arguments[f]; for (d in e) a[d] = e[d]; for (var g = 0; g < La.length; g++)d = La[g], Object.prototype.hasOwnProperty.call(e, d) && (a[d] = e[d]) } };\r\n                var Na; _.Oa = function () { return _.v(\"Safari\") && !(Na() || _.v(\"Coast\") || _.v(\"Opera\") || _.v(\"Edge\") || _.v(\"Silk\") || _.v(\"Android\")) }; Na = function () { return (_.v(\"Chrome\") || _.v(\"CriOS\")) && !_.v(\"Edge\") }; _.Pa = function () { return _.v(\"Android\") && !(Na() || _.v(\"Firefox\") || _.v(\"Opera\") || _.v(\"Silk\")) };\r\n                _.Qa = function () { return _.v(\"iPhone\") && !_.v(\"iPod\") && !_.v(\"iPad\") }; _.Ra = function () { return _.Qa() || _.v(\"iPad\") || _.v(\"iPod\") }; _.Sa = function (a) { _.Sa[\" \"](a); return a }; _.Sa[\" \"] = _.ia; var Ua = function (a, c) { var d = Ta; return Object.prototype.hasOwnProperty.call(d, a) ? d[a] : d[a] = c(a) }; var hb, ib, Ta, qb; _.Va = _.v(\"Opera\"); _.x = _.v(\"Trident\") || _.v(\"MSIE\"); _.Wa = _.v(\"Edge\"); _.Xa = _.Wa || _.x; _.Ya = _.v(\"Gecko\") && !(-1 != _.Ha.toLowerCase().indexOf(\"webkit\") && !_.v(\"Edge\")) && !(_.v(\"Trident\") || _.v(\"MSIE\")) && !_.v(\"Edge\"); _.Za = -1 != _.Ha.toLowerCase().indexOf(\"webkit\") && !_.v(\"Edge\"); _.$a = _.v(\"Macintosh\"); _.ab = _.v(\"Windows\"); _.bb = _.v(\"Linux\") || _.v(\"CrOS\"); _.cb = _.v(\"Android\"); _.db = _.Qa(); _.eb = _.v(\"iPad\"); _.fb = _.v(\"iPod\"); _.gb = _.Ra(); hb = function () { var a = _.m.document; return a ? a.documentMode : void 0 };\r\n                a: { var jb = \"\", kb = function () { var a = _.Ha; if (_.Ya) return /rv\\:([^\\);]+)(\\)|;)/.exec(a); if (_.Wa) return /Edge\\/([\\d\\.]+)/.exec(a); if (_.x) return /\\b(?:MSIE|rv)[: ]([^\\);]+)(\\)|;)/.exec(a); if (_.Za) return /WebKit\\/(\\S+)/.exec(a); if (_.Va) return /(?:Version)[ \\/]?(\\S+)/.exec(a) }(); kb && (jb = kb ? kb[1] : \"\"); if (_.x) { var lb = hb(); if (null != lb && lb > (0, window.parseFloat)(jb)) { ib = String(lb); break a } } ib = jb } _.mb = ib; Ta = {}; _.nb = function (a) { return Ua(a, function () { return 0 <= _.Aa(_.mb, a) }) }; _.pb = function (a) { return Number(ob) >= a }; var rb = _.m.document; qb = rb && _.x ? hb() || (\"CSS1Compat\" == rb.compatMode ? (0, window.parseInt)(_.mb, 10) : 5) : void 0; var ob = qb;\r\n                _.sb = _.v(\"Firefox\"); _.tb = _.Qa() || _.v(\"iPod\"); _.ub = _.v(\"iPad\"); _.vb = _.Pa(); _.wb = Na(); _.xb = _.Oa() && !_.Ra(); var yb = null; var Bb, Db; _.y = function () { }; _.zb = \"function\" == typeof window.Uint8Array; _.z = function (a, c, d, e, f) { a.b = null; c || (c = d ? [d] : []); a.G = d ? String(d) : void 0; a.w = 0 === d ? -1 : 0; a.f = c; a: { if (a.f.length && (c = a.f.length - 1, (d = a.f[c]) && \"object\" == typeof d && !_.la(d) && !(_.zb && d instanceof window.Uint8Array))) { a.A = c - a.w; a.o = d; break a } -1 < e ? (a.A = e, a.o = null) : a.A = Number.MAX_VALUE } a.D = {}; if (f) for (e = 0; e < f.length; e++)c = f[e], c < a.A ? (c += a.w, a.f[c] = a.f[c] || _.Ab) : (Bb(a), a.o[c] = a.o[c] || _.Ab) }; _.Ab = [];\r\n                Bb = function (a) { var c = a.A + a.w; a.f[c] || (a.o = a.f[c] = {}) }; _.A = function (a, c) { if (c < a.A) { c += a.w; var d = a.f[c]; return d === _.Ab ? a.f[c] = [] : d } if (a.o) return d = a.o[c], d === _.Ab ? a.o[c] = [] : d }; _.Cb = function (a, c, d) { a = _.A(a, c); return null == a ? d : a }; _.B = function (a, c, d) { c < a.A ? a.f[c + a.w] = d : (Bb(a), a.o[c] = d) }; _.C = function (a, c, d) { a.b || (a.b = {}); if (!a.b[d]) { var e = _.A(a, d); e && (a.b[d] = new c(e)) } return a.b[d] }; Db = function (a) { if (a.b) for (var c in a.b) { var d = a.b[c]; if (_.la(d)) for (var e = 0; e < d.length; e++)d[e] && d[e].Pb(); else d && d.Pb() } };\r\n                _.y.prototype.Pb = function () { Db(this); return this.f };\r\n                _.y.prototype.j = _.zb ? function () {\r\n                    var a = window.Uint8Array.prototype.toJSON; window.Uint8Array.prototype.toJSON = function () { if (!yb) { yb = {}; for (var a = 0; 65 > a; a++)yb[a] = \"ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/=\".charAt(a) } a = yb; for (var c = [], f = 0; f < this.length; f += 3) { var g = this[f], h = f + 1 < this.length, l = h ? this[f + 1] : 0, q = f + 2 < this.length, r = q ? this[f + 2] : 0, w = g >> 2; g = (g & 3) << 4 | l >> 4; l = (l & 15) << 2 | r >> 6; r &= 63; q || (r = 64, h || (l = 64)); c.push(a[w], a[g], a[l], a[r]) } return c.join(\"\") }; try {\r\n                        var c = JSON.stringify(this.Pb(),\r\n                            Eb)\r\n                    } finally { window.Uint8Array.prototype.toJSON = a } return c\r\n                } : function () { return JSON.stringify(this.Pb(), Eb) }; var Eb = function (a, c) { if (_.fa(c)) { if ((0, window.isNaN)(c)) return \"NaN\"; if (window.Infinity === c) return \"Infinity\"; if (-window.Infinity === c) return \"-Infinity\" } return c }; _.y.prototype.toString = function () { Db(this); return this.f.toString() };\r\n                _.D = function () { this.La = this.La; this.Lb = this.Lb }; _.D.prototype.La = !1; _.D.prototype.ka = function () { this.La || (this.La = !0, this.N()) }; _.D.prototype.N = function () { if (this.Lb) for (; this.Lb.length;)this.Lb.shift()() }; var Fb = function (a) { _.z(this, a, 0, -1, null) }; _.u(Fb, _.y); _.Gb = function (a) { _.z(this, a, 0, -1, null) }; _.u(_.Gb, _.y); _.Hb = function (a) { _.z(this, a, 0, -1, null) }; _.u(_.Hb, _.y); var Ib = function (a) { _.D.call(this); this.j = a; this.b = []; this.f = {} }; _.u(Ib, _.D); Ib.prototype.ld = function () { for (var a = this.b.length, c = this.b, d = [], e = 0; e < a; ++e) { var f = c[e].b(); a: { var g = this.j; for (var h = f.split(\".\"), l = h.length, q = 0; q < l; ++q)if (g[h[q]]) g = g[h[q]]; else { g = null; break a } g = g instanceof Function ? g : null } if (g && g != this.f[f]) try { c[e].ld(g) } catch (r) { } else d.push(c[e]) } this.b = d.concat(c.slice(a)) };\r\n                var Jb = function (a) { _.D.call(this); this.A = a; this.j = this.b = null; this.w = 0; this.o = {}; this.f = !1; a = window.navigator.userAgent; 0 <= a.indexOf(\"MSIE\") && 0 <= a.indexOf(\"Trident\") && (a = /\\b(?:MSIE|rv)[: ]([^\\);]+)(\\)|;)/.exec(a)) && a[1] && 9 > (0, window.parseFloat)(a[1]) && (this.f = !0) }; _.u(Jb, _.D); Jb.prototype.B = function (a, c) { this.b = c; this.j = a; c.preventDefault ? c.preventDefault() : c.returnValue = !1 };\r\n                _.Kb = function (a) { _.z(this, a, 0, -1, null) }; _.u(_.Kb, _.y); _.Lb = function (a) { _.z(this, a, 0, -1, null) }; _.u(_.Lb, _.y); _.E = function (a, c) { return null != a ? !!a : !!c }; _.F = function (a, c) { void 0 == c && (c = \"\"); return null != a ? a : c }; _.Mb = function (a, c) { void 0 == c && (c = 0); return null != a ? a : c }; var Nb, Qb, Pb; _.Ob = function (a) { var c = \"https://www.google.com/gen_204?\"; c += a.j(2040 - c.length); Nb(c) }; Nb = function (a) { var c = new window.Image, d = Pb; c.onerror = c.onload = c.onabort = function () { d in Qb && delete Qb[d] }; Qb[Pb++] = c; c.src = a }; Qb = []; Pb = 0; var Rb = function (a) { this.b = a }; Rb.prototype.log = function (a, c) { try { if (this.B(a)) { var d = this.j(a, c); this.f(d) } } catch (e) { } }; Rb.prototype.f = function (a) { this.b ? a.b() : _.Ob(a) }; _.Sb = function () { this.data = {} }; _.Sb.prototype.b = function () { window.console && window.console.log && window.console.log(\"Log data: \", this.data) }; _.Sb.prototype.j = function (a) { var c = [], d; for (d in this.data) c.push((0, window.encodeURIComponent)(d) + \"=\" + (0, window.encodeURIComponent)(String(this.data[d]))); return (\"atyp=i&zx=\" + (new Date).getTime() + \"&\" + c.join(\"&\")).substr(0, a) };\r\n                var Tb = function (a, c) { this.data = {}; var d = _.C(a, Fb, 8) || new Fb; this.data.ei = _.F(_.A(a, 10)); this.data.ogf = _.F(_.A(d, 3)); var e = window.google && window.google.sn ? /.*hp$/.test(window.google.sn) ? !1 : !0 : _.E(_.A(a, 7)); this.data.ogrp = e ? \"1\" : \"\"; this.data.ogv = _.F(_.A(d, 6)) + \".\" + _.F(_.A(d, 7)); this.data.ogd = _.F(_.A(a, 21)); this.data.ogc = _.F(_.A(a, 20)); this.data.ogl = _.F(_.A(a, 5)); c && (this.data.oggv = c) }; _.u(Tb, _.Sb);\r\n                _.Ub = function (a, c, d, e, f) { Tb.call(this, a, c); _.Ma(this.data, { jexpid: _.F(_.A(a, 9)), srcpg: \"prop=\" + _.F(_.A(a, 6)), jsr: Math.round(1 / e), emsg: d.name + \":\" + d.message }); if (f) { f._sn && (f._sn = \"og.\" + f._sn); for (var g in f) this.data[(0, window.encodeURIComponent)(g)] = f[g] } }; _.u(_.Ub, Tb);\r\n                var Vb = function (a) { _.z(this, a, 0, -1, null) }; _.u(Vb, _.y); _.Wb = function (a) { _.z(this, a, 0, -1, null) }; _.u(_.Wb, _.y); var bc; _.Xb = function () { this.b = {}; this.f = {} }; _.ja(_.Xb); _.Zb = function (a, c) { var d = _.Xb.ra(); if (a in d.b) { if (d.b[a] != c) throw new Yb; } else { d.b[a] = c; if (c = d.f[a]) for (var e = 0, f = c.length; e < f; e++)c[e].b(d.b, a); delete d.f[a] } }; _.ac = function (a, c) { if (c in a.b) return a.b[c]; throw new $b; }; bc = function () { _.va.call(this) }; _.u(bc, _.va); var Yb = function () { _.va.call(this) }; _.u(Yb, bc); var $b = function () { _.va.call(this) }; _.u($b, bc);\r\n                var cc = function (a, c, d, e) { this.b = e; this.La = c; this.J = d; this.w = _.Mb(+_.Cb(a, 2, .001), .001); this.G = _.E(_.A(a, 1)) && Math.random() < this.w; this.C = _.Mb(_.Cb(a, 3, 1), 1); this.A = 0; this.o = null; this.D = _.E(_.Cb(a, 4, !0), !0) }; _.u(cc, Rb); cc.prototype.log = function (a, c) { cc.H.log.call(this, a, c); if (this.b && this.D) throw a; }; cc.prototype.B = function () { return this.b || this.G && this.A < this.C }; cc.prototype.j = function (a, c) { try { return (this.o || _.ac(_.Xb.ra(), \"lm\")).b(a, c) } catch (d) { return new _.Ub(this.La, this.J, a, this.w, c) } }; cc.prototype.f = function (a) { cc.H.f.call(this, a); this.A++ };\r\n                var dc = [1, 2, 3, 4, 5, 6, 9, 10, 11, 13, 14, 28, 29, 30, 34, 35, 37, 38, 39, 40, 41, 42, 43, 48, 49, 50, 51, 52, 53, 55, 56, 57, 58, 59, 500], fc = function (a, c, d, e, f, g) { Tb.call(this, a, c); _.Ma(this.data, { oge: e, ogex: _.F(_.A(a, 9)), ogp: _.F(_.A(a, 6)), ogsr: Math.round(1 / (ec(e) ? _.Mb(+_.Cb(d, 3, 1)) : _.Mb(+_.Cb(d, 2, 1E-4)))), ogus: f }); if (g) { \"ogw\" in g && (this.data.ogw = g.ogw, delete g.ogw); \"ved\" in g && (this.data.ved = g.ved, delete g.ved); a = []; for (var h in g) 0 != a.length && a.push(\",\"), a.push((h + \"\").replace(\".\", \"%2E\").replace(\",\", \"%2C\")), a.push(\".\"), a.push((g[h] + \"\").replace(\".\", \"%2E\").replace(\",\", \"%2C\")); g = a.join(\"\"); \"\" != g && (this.data.ogad = g) } }; _.u(fc, Tb); var gc = null, ec = function (a) { if (!gc) { gc = {}; for (var c = 0; c < dc.length; c++)gc[dc[c]] = !0 } return !!gc[a] };\r\n                var hc = function (a, c, d, e, f) { this.b = f; this.G = a; this.D = c; this.La = e; this.C = _.Mb(+_.Cb(a, 2, 1E-4), 1E-4); this.A = _.Mb(+_.Cb(a, 3, 1), 1); c = Math.random(); this.w = _.E(_.A(a, 1)) && c < this.C; this.o = _.E(_.A(a, 1)) && c < this.A; a = 0; _.E(_.A(d, 1)) && (a |= 1); _.E(_.A(d, 2)) && (a |= 2); _.E(_.A(d, 3)) && (a |= 4); this.J = a }; _.u(hc, Rb); hc.prototype.B = function (a) { return this.b || (ec(a) ? this.o : this.w) }; hc.prototype.j = function (a, c) { return new fc(this.D, this.La, this.G, a, this.J, c) };\r\n                var ic = function (a) { this.b = a; this.f = void 0; this.j = [] }; ic.prototype.then = function (a, c, d) { this.j.push(new jc(a, c, d)); _.kc(this) }; _.kc = function (a) { if (0 < a.j.length) { var c = void 0 !== a.b, d = void 0 !== a.f; if (c || d) { c = c ? a.o : a.A; d = a.j; a.j = []; try { (0, _.Ca)(d, c, a) } catch (e) { window.console.error(e) } } } }; ic.prototype.o = function (a) { a.f && a.f.call(a.b, this.b) }; ic.prototype.A = function (a) { a.j && a.j.call(a.b, this.f) }; var jc = function (a, c, d) { this.f = a; this.j = c; this.b = d };\r\n                _.G = function () { this.b = new ic; this.f = new ic; this.w = new ic; this.o = new ic; this.A = new ic; this.B = new ic; this.C = new ic; this.j = new ic }; _.ja(_.G); _.k = _.G.prototype; _.k.Eg = function () { return this.b }; _.k.Ng = function () { return this.f }; _.k.Sg = function () { return this.w }; _.k.Lg = function () { return this.o }; _.k.Qg = function () { return this.A }; _.k.Tg = function () { return this.B }; _.k.Ig = function () { return this.C }; _.k.Jg = function () { return this.j };\r\n                var lc = function (a) { _.z(this, a, 0, -1, null) }; _.u(lc, _.y); _.nc = function () { return _.C(_.mc, _.Gb, 1) }; _.oc = function () { return _.C(_.mc, _.Hb, 5) }; var pc; window.gbar_ && window.gbar_.CONFIG ? pc = window.gbar_.CONFIG[0] || {} : pc = []; _.mc = new lc(pc); _.t(\"gbar_._DumpException\", function (a) { if (this._D) throw a; _.H ? _.H.log(a) : window.console.error(a) }); var qc, rc, sc, tc, uc; qc = _.C(_.mc, _.Wb, 3) || new _.Wb; rc = _.nc() || new _.Gb; _.H = new cc(qc, rc, \"quantum:gapiBuildLabel\", !1); sc = _.nc() || new _.Gb; tc = _.oc() || new _.Hb; uc = _.C(_.mc, Vb, 4) || new Vb; _.vc = new hc(uc, sc, tc, \"quantum:gapiBuildLabel\", !1); _.wc = new Jb(_.H); _.vc.log(8, { m: \"BackCompat\" == window.document.compatMode ? \"q\" : \"s\" }); _.t(\"gbar.A\", ic); ic.prototype.aa = ic.prototype.then; _.t(\"gbar.B\", _.G); _.G.prototype.ba = _.G.prototype.Ng; _.G.prototype.bb = _.G.prototype.Sg; _.G.prototype.bd = _.G.prototype.Qg; _.G.prototype.be = _.G.prototype.Tg; _.G.prototype.bf = _.G.prototype.Eg; _.G.prototype.bg = _.G.prototype.Lg; _.G.prototype.bh = _.G.prototype.Ig; _.G.prototype.bi = _.G.prototype.Jg; _.t(\"gbar.a\", _.G.ra()); var xc = new Ib(window); _.Zb(\"api\", xc); var yc = _.oc() || new _.Hb, zc = _.F(_.A(yc, 8)); window.__PVT = zc; _.Zb(\"eq\", _.wc);\r\n\r\n            } catch (e) { _._DumpException(e) }\r\n            /* _Module_:syf */\r\n            try {\r\n                _.Ac = !_.x || _.pb(9); _.Bc = !_.x || _.pb(9); _.Cc = _.x && !_.nb(\"9\"); !_.Za || _.nb(\"528\"); _.Ya && _.nb(\"1.9b\") || _.x && _.nb(\"8\") || _.Va && _.nb(\"9.5\") || _.Za && _.nb(\"528\"); _.Ya && !_.nb(\"8\") || _.x && _.nb(\"9\"); _.Dc = function () { if (!_.m.addEventListener || !Object.defineProperty) return !1; var a = !1, c = Object.defineProperty({}, \"passive\", { get: function () { a = !0 } }); _.m.addEventListener(\"test\", _.ia, c); _.m.removeEventListener(\"test\", _.ia, c); return a }();\r\n                var Ec; Ec = function (a) { return _.Za ? \"webkit\" + a : _.Va ? \"o\" + a.toLowerCase() : a.toLowerCase() }; _.Fc = _.x ? \"focusin\" : \"DOMFocusIn\"; _.Gc = Ec(\"AnimationEnd\"); _.Hc = Ec(\"TransitionEnd\");\r\n            } catch (e) { _._DumpException(e) }\r\n            /* _Module_:qhga */\r\n            try {\r\n                var Ic = function (a) { _.z(this, a, 0, -1, null) }; _.u(Ic, _.y); var Jc = function () { _.D.call(this); this.f = []; this.b = [] }; _.u(Jc, _.D); Jc.prototype.j = function (a, c) { this.f.push({ md: a, options: c }) }; Jc.prototype.init = function (a, c, d) { window.gapi = {}; var e = window.___jsl = {}; e.h = _.F(_.A(a, 1)); e.ms = _.F(_.A(a, 2)); e.m = _.F(_.A(a, 3)); e.l = []; _.A(c, 1) && (a = _.A(c, 3)) && this.b.push(a); _.A(d, 1) && (d = _.A(d, 2)) && this.b.push(d); _.t(\"gapi.load\", (0, _.p)(this.j, this)); return this };\r\n                var Kc = _.C(_.mc, _.Kb, 14) || new _.Kb, Lc = _.C(_.mc, _.Lb, 9) || new _.Lb, Mc = new Ic, Nc = new Jc; Nc.init(Kc, Lc, Mc); _.Zb(\"gs\", Nc);\r\n            } catch (e) { _._DumpException(e) }\r\n            /* _GlobalSuffix_ */\r\n        })(this.gbar_);\r\n// Google Inc.\r\n    </script>\r\n    <script async=\"\" src=\"https://www.gstatic.com/feedback/js/help/prod/service/screenshot.min.js\"></script>\r\n    <script src=\"//www.google.com/tools/feedback/metric/report?elapsedMillis=241&amp;userAction=describe_step_shown&amp;gfSessionId=1506725712427-7151642501230186178&amp;at&amp;productId=713678&amp;bucket=ExternalUserData&amp;locale=de&amp;flow=feedback.web.material&amp;productSpecificContext=https%3A%2F%2Fdocs.google.com%2Fforms%2Fd%2Fe%2F1FAIpQLSe2uZzZU0xKyIOCXPfo6kdklnDnrQf9qipNMaVQAaI-vW7h2w%2Fviewform&amp;useAnonymousMetrics=false&amp;callback=_callbacks___0j86hl3qo\"></script>\r\n    <script src=\"//www.google.com/tools/feedback/metric/report?elapsedMillis=244&amp;subUserAction=ui_ready_from_start&amp;userAction=latency_measured&amp;gfSessionId=1506725712427-7151642501230186178&amp;at&amp;productId=713678&amp;bucket=ExternalUserData&amp;locale=de&amp;flow=feedback.web.material&amp;productSpecificContext=https%3A%2F%2Fdocs.google.com%2Fforms%2Fd%2Fe%2F1FAIpQLSe2uZzZU0xKyIOCXPfo6kdklnDnrQf9qipNMaVQAaI-vW7h2w%2Fviewform&amp;useAnonymousMetrics=false&amp;callback=_callbacks___1j86hl3qr\"></script>\r\n    <script src=\"//www.google.com/tools/feedback/metric/report?elapsedMillis=53&amp;subUserAction=loadjs_execute_time&amp;userAction=latency_measured&amp;gfSessionId=1506725712427-7151642501230186178&amp;at&amp;productId=713678&amp;bucket=ExternalUserData&amp;locale=de&amp;flow=feedback.web.material&amp;productSpecificContext=https%3A%2F%2Fdocs.google.com%2Fforms%2Fd%2Fe%2F1FAIpQLSe2uZzZU0xKyIOCXPfo6kdklnDnrQf9qipNMaVQAaI-vW7h2w%2Fviewform&amp;useAnonymousMetrics=false&amp;callback=_callbacks___2j86hl3qs\"></script>\r\n    <script src=\"//www.google.com/tools/feedback/metric/report?elapsedMillis=245&amp;subUserAction=loadjs_start_time&amp;userAction=latency_measured&amp;gfSessionId=1506725712427-7151642501230186178&amp;at&amp;productId=713678&amp;bucket=ExternalUserData&amp;locale=de&amp;flow=feedback.web.material&amp;productSpecificContext=https%3A%2F%2Fdocs.google.com%2Fforms%2Fd%2Fe%2F1FAIpQLSe2uZzZU0xKyIOCXPfo6kdklnDnrQf9qipNMaVQAaI-vW7h2w%2Fviewform&amp;useAnonymousMetrics=false&amp;callback=_callbacks___3j86hl3qs\"></script>\r\n    <script src=\"//www.google.com/tools/feedback/metric/report?elapsedMillis=15&amp;subUserAction=loadjs_parse_time&amp;userAction=latency_measured&amp;gfSessionId=1506725712427-7151642501230186178&amp;at&amp;productId=713678&amp;bucket=ExternalUserData&amp;locale=de&amp;flow=feedback.web.material&amp;productSpecificContext=https%3A%2F%2Fdocs.google.com%2Fforms%2Fd%2Fe%2F1FAIpQLSe2uZzZU0xKyIOCXPfo6kdklnDnrQf9qipNMaVQAaI-vW7h2w%2Fviewform&amp;useAnonymousMetrics=false&amp;callback=_callbacks___4j86hl3qs\"></script>\r\n    <script src=\"//www.google.com/tools/feedback/metric/report?elapsedMillis=4&amp;subUserAction=loadjs_connect_time&amp;userAction=latency_measured&amp;gfSessionId=1506725712427-7151642501230186178&amp;at&amp;productId=713678&amp;bucket=ExternalUserData&amp;locale=de&amp;flow=feedback.web.material&amp;productSpecificContext=https%3A%2F%2Fdocs.google.com%2Fforms%2Fd%2Fe%2F1FAIpQLSe2uZzZU0xKyIOCXPfo6kdklnDnrQf9qipNMaVQAaI-vW7h2w%2Fviewform&amp;useAnonymousMetrics=false&amp;callback=_callbacks___5j86hl3qt\"></script>\r\n    <script src=\"//www.google.com/tools/feedback/metric/report?elapsedMillis=136&amp;subUserAction=loadjs_request_time&amp;userAction=latency_measured&amp;gfSessionId=1506725712427-7151642501230186178&amp;at&amp;productId=713678&amp;bucket=ExternalUserData&amp;locale=de&amp;flow=feedback.web.material&amp;productSpecificContext=https%3A%2F%2Fdocs.google.com%2Fforms%2Fd%2Fe%2F1FAIpQLSe2uZzZU0xKyIOCXPfo6kdklnDnrQf9qipNMaVQAaI-vW7h2w%2Fviewform&amp;useAnonymousMetrics=false&amp;callback=_callbacks___6j86hl3qt\"></script>\r\n    <script src=\"//www.google.com/tools/feedback/metric/report?elapsedMillis=38&amp;subUserAction=loadjs_response_time&amp;userAction=latency_measured&amp;gfSessionId=1506725712427-7151642501230186178&amp;at&amp;productId=713678&amp;bucket=ExternalUserData&amp;locale=de&amp;flow=feedback.web.material&amp;productSpecificContext=https%3A%2F%2Fdocs.google.com%2Fforms%2Fd%2Fe%2F1FAIpQLSe2uZzZU0xKyIOCXPfo6kdklnDnrQf9qipNMaVQAaI-vW7h2w%2Fviewform&amp;useAnonymousMetrics=false&amp;callback=_callbacks___7j86hl3qt\"></script>\r\n    <script src=\"//www.google.com/tools/feedback/metric/report?elapsedMillis=34&amp;subUserAction=action_request&amp;userAction=snapshot_captured&amp;gfSessionId=0-7151642501230186178&amp;at&amp;productId=713678&amp;bucket=undefined&amp;locale=en&amp;flow=feedback.web.material&amp;productSpecificContext=https%3A%2F%2Fdocs.google.com%2Fforms%2Fd%2Fe%2F1FAIpQLSe2uZzZU0xKyIOCXPfo6kdklnDnrQf9qipNMaVQAaI-vW7h2w%2Fviewform&amp;useAnonymousMetrics=false&amp;callback=_callbacks___0j86hl3ss\"></script>\r\n    <script src=\"//www.google.com/tools/feedback/metric/report?elapsedMillis=40&amp;subUserAction=action_success&amp;userAction=snapshot_captured&amp;gfSessionId=0-7151642501230186178&amp;at&amp;productId=713678&amp;bucket=undefined&amp;locale=en&amp;flow=feedback.web.material&amp;productSpecificContext=https%3A%2F%2Fdocs.google.com%2Fforms%2Fd%2Fe%2F1FAIpQLSe2uZzZU0xKyIOCXPfo6kdklnDnrQf9qipNMaVQAaI-vW7h2w%2Fviewform&amp;useAnonymousMetrics=false&amp;callback=_callbacks___1j86hl3tw\"></script>\r\n    <script src=\"//www.google.com/tools/feedback/metric/report?elapsedMillis=98&amp;subUserAction=feedback_snapshot_displayed&amp;userAction=latency_measured&amp;gfSessionId=0-7151642501230186178&amp;at&amp;productId=713678&amp;bucket=undefined&amp;locale=en&amp;flow=feedback.web.material&amp;productSpecificContext=https%3A%2F%2Fdocs.google.com%2Fforms%2Fd%2Fe%2F1FAIpQLSe2uZzZU0xKyIOCXPfo6kdklnDnrQf9qipNMaVQAaI-vW7h2w%2Fviewform&amp;useAnonymousMetrics=false&amp;callback=_callbacks___2j86hl3uj\"></script>\r\n    <script src=\"//www.google.com/tools/feedback/metric/report?elapsedMillis=218&amp;subUserAction=feedback_annotator_ready&amp;userAction=latency_measured&amp;gfSessionId=0-7151642501230186178&amp;at&amp;productId=713678&amp;bucket=undefined&amp;locale=en&amp;flow=feedback.web.material&amp;productSpecificContext=https%3A%2F%2Fdocs.google.com%2Fforms%2Fd%2Fe%2F1FAIpQLSe2uZzZU0xKyIOCXPfo6kdklnDnrQf9qipNMaVQAaI-vW7h2w%2Fviewform&amp;useAnonymousMetrics=false&amp;callback=_callbacks___3j86hl3xv\"></script>\r\n    <script src=\"//www.google.com/tools/feedback/metric/report?elapsedMillis=238&amp;subUserAction=action_request&amp;userAction=create_screenshot_renderer&amp;gfSessionId=0-7151642501230186178&amp;at&amp;productId=713678&amp;bucket=undefined&amp;locale=en&amp;flow=feedback.web.material&amp;productSpecificContext=https%3A%2F%2Fdocs.google.com%2Fforms%2Fd%2Fe%2F1FAIpQLSe2uZzZU0xKyIOCXPfo6kdklnDnrQf9qipNMaVQAaI-vW7h2w%2Fviewform&amp;useAnonymousMetrics=false&amp;callback=_callbacks___4j86hl3yg\"></script>\r\n    <script src=\"//www.google.com/tools/feedback/metric/report?elapsedMillis=28&amp;subUserAction=action_success&amp;userAction=create_screenshot_renderer&amp;gfSessionId=0-7151642501230186178&amp;at&amp;productId=713678&amp;bucket=undefined&amp;locale=en&amp;flow=feedback.web.material&amp;productSpecificContext=https%3A%2F%2Fdocs.google.com%2Fforms%2Fd%2Fe%2F1FAIpQLSe2uZzZU0xKyIOCXPfo6kdklnDnrQf9qipNMaVQAaI-vW7h2w%2Fviewform&amp;useAnonymousMetrics=false&amp;callback=_callbacks___5j86hl3z8\"></script>\r\n    <script src=\"//www.google.com/tools/feedback/metric/report?elapsedMillis=503&amp;subUserAction=action_request&amp;userAction=render_screenshot&amp;gfSessionId=0-7151642501230186178&amp;at&amp;productId=713678&amp;bucket=undefined&amp;locale=en&amp;flow=feedback.web.material&amp;productSpecificContext=https%3A%2F%2Fdocs.google.com%2Fforms%2Fd%2Fe%2F1FAIpQLSe2uZzZU0xKyIOCXPfo6kdklnDnrQf9qipNMaVQAaI-vW7h2w%2Fviewform&amp;useAnonymousMetrics=false&amp;callback=_callbacks___6j86hl45s\"></script>\r\n    <script src=\"//www.google.com/tools/feedback/metric/report?elapsedMillis=234&amp;subUserAction=action_success&amp;userAction=render_screenshot&amp;gfSessionId=0-7151642501230186178&amp;at&amp;productId=713678&amp;bucket=undefined&amp;locale=en&amp;flow=feedback.web.material&amp;productSpecificContext=https%3A%2F%2Fdocs.google.com%2Fforms%2Fd%2Fe%2F1FAIpQLSe2uZzZU0xKyIOCXPfo6kdklnDnrQf9qipNMaVQAaI-vW7h2w%2Fviewform&amp;useAnonymousMetrics=false&amp;callback=_callbacks___7j86hl4cc\"></script>\r\n    <script src=\"//www.google.com/tools/feedback/metric/report?elapsedMillis=2431&amp;userAction=dismissed&amp;gfSessionId=1506725712427-7151642501230186178&amp;at&amp;productId=713678&amp;bucket=ExternalUserData&amp;locale=de&amp;flow=feedback.web.material&amp;productSpecificContext=https%3A%2F%2Fdocs.google.com%2Fforms%2Fd%2Fe%2F1FAIpQLSe2uZzZU0xKyIOCXPfo6kdklnDnrQf9qipNMaVQAaI-vW7h2w%2Fviewform&amp;useAnonymousMetrics=false&amp;callback=_callbacks___8j86hl5fj\"></script>\r\n</head>\r\n\r\n<div id=\"contact-form\" class=\"clearfix\">\r\n    <form method=\"post\" action=\"process.php\">\r\n        <div class=\"freebirdFormviewerViewFormCard\">\r\n\r\n            <div class=\"freebirdFormviewerViewAccentBanner freebirdAccentBackground\"></div>\r\n            <div class=\"freebirdFormviewerViewFormContent \">\r\n                <div class=\"freebirdFormviewerViewHeaderHeader\">\r\n                    <div class=\"freebirdFormviewerViewHeaderTitleRow\">\r\n                        <div class=\"freebirdFormviewerViewHeaderTitle\" dir=\"auto\" role=\"heading\" aria-level=\"1\">Anmeldung Leapin' Lindy 2018</div>\r\n                    </div>\r\n                    <div class=\"freebirdFormviewerViewHeaderRequiredLegend\" aria-hidden=\"true\" dir=\"auto\">* Erforderlich</div>\r\n                </div>\r\n                <div class=\"freebirdFormviewerViewItemList\" role=\"list\">\r\n                    <div role=\"listitem\" class=\"freebirdFormviewerViewItemsItemItem freebirdFormviewerViewItemsTextTextItem freebirdFormviewerViewEmailCollectionField\" jsname=\"ibnC6b\" jscontroller=\"rDGJeb\" jsaction=\"sPvj8e:e4JwSe,vwKRrd;\" data-required=\"true\" data-validation-operation=\"102\" data-validation-type=\"2\">\r\n                        <div class=\"freebirdFormviewerViewItemsItemItemHeader\">\r\n                            <div class=\"freebirdFormviewerViewItemsItemItemTitleContainer\">\r\n                                <div class=\"freebirdFormviewerViewItemsItemItemTitle\" id=\"i1\" dir=\"auto\" role=\"heading\" aria-level=\"2\">\r\n                                    E-Mail-Adresse <span class=\"freebirdFormviewerViewItemsItemRequiredAsterisk\" aria-hidden=\"true\">*</span>\r\n                                </div>\r\n                            </div>\r\n                        </div>\r\n                        <div class=\"freebirdFormviewerViewItemsTextItemWrapper\">\r\n                            <div class=\"quantumWizTextinputPaperinputEl freebirdFormviewerViewItemsTextShortText freebirdFormviewerViewItemsTextEmail freebirdThemedInput\" jscontroller=\"pxq3x\" jsaction=\"clickonly:KjsqPd; focus:Jt1EX; blur:fpfTEe; input:Lg5SV;\" jsshadow=\"\" jsname=\"W85ice\" title=\"E-Mail-Adresse\">\r\n                                <div class=\"quantumWizTextinputPaperinputMainContent exportContent\">\r\n                                    <div class=\"quantumWizTextinputPaperinputContentArea exportContentArea\">\r\n                                        <div class=\"quantumWizTextinputPaperinputInputArea\">\r\n                                            <input type=\"email\" class=\"quantumWizTextinputPaperinputInput exportInput\" jsname=\"YPqjbf\" autocomplete=\"email\" tabindex=\"0\" aria-label=\"Ihre E-Mail-Adresse\" name=\"emailAddress\" value=\"\" required=\"\" dir=\"auto\" data-initial-dir=\"auto\" data-initial-value=\"\">\r\n                                            <div jsname=\"LwH6nd\" class=\"quantumWizTextinputPaperinputPlaceholder exportLabel\" aria-hidden=\"true\">Ihre E-Mail-Adresse</div>\r\n                                        </div>\r\n                                        <div class=\"quantumWizTextinputPaperinputUnderline exportUnderline\"></div>\r\n                                        <div jsname=\"XmnwAc\" class=\"quantumWizTextinputPaperinputFocusUnderline exportFocusUnderline\"></div>\r\n                                    </div>\r\n                                </div>\r\n                                <div class=\"quantumWizTextinputPaperinputCounterErrorHolder\">\r\n                                    <div jsname=\"ty6ygf\" class=\"quantumWizTextinputPaperinputHint exportHint\"></div>\r\n                                </div>\r\n                            </div>\r\n                        </div>\r\n                        <div jsname=\"XbIQze\" class=\"freebirdFormviewerViewItemsItemErrorMessage\" id=\"i2\" role=\"alert\"></div>\r\n                    </div>\r\n                    <div role=\"listitem\" class=\"freebirdFormviewerViewItemsItemItem\" jsname=\"ibnC6b\" jscontroller=\"pkFYWb\" jsaction=\"sPvj8e:F0ZU4;\" data-required=\"true\" data-other-input=\"qSV85\" data-other-hidden=\"MfYA1e\" data-item-id=\"297145218\">\r\n                        <div class=\"freebirdFormviewerViewItemsItemItemHeader\">\r\n                            <div class=\"freebirdFormviewerViewItemsItemItemTitleContainer\">\r\n                                <div class=\"freebirdFormviewerViewItemsItemItemTitle\" dir=\"auto\" id=\"i3\" role=\"heading\" aria-level=\"2\" aria-describedby=\"i.desc.297145218\">\r\n                                    Was m�chtest du buchen? <span class=\"freebirdFormviewerViewItemsItemRequiredAsterisk\" aria-hidden=\"true\">*</span>\r\n                                </div>\r\n                                <div class=\"freebirdFormviewerViewItemsItemItemHelpText\" id=\"i.desc.297145218\" dir=\"auto\"></div>\r\n                            </div>\r\n                        </div>\r\n                        <div jscontroller=\"eFy6Rc\" jsaction=\"sPvj8e:Gh295d\" jsname=\"cnAzRb\" data-input=\"L9xHkb\">\r\n                            <div jscontroller=\"wPRNsd\" jsshadow=\"\" jsaction=\"keydown: I481le;JIbuQc:JIbuQc;rcuQ6b:rcuQ6b\" jsname=\"wCJL8\" aria-labelledby=\"i3\" aria-describedby=\"i.desc.297145218 i.err.297145218\" aria-required=\"true\" role=\"radiogroup\">\r\n                                <content role=\"presentation\" jsname=\"bN97Pc\">\r\n                                    <div class=\"\">\r\n                                        <label class=\"docssharedWizToggleLabeledContainer freebirdFormviewerViewItemsRadioChoice\">\r\n                                            <div class=\"exportLabelWrapper\">\r\n                                                <div class=\"quantumWizTogglePaperradioEl docssharedWizToggleLabeledControl freebirdThemedRadio freebirdThemedRadioDarkerDisabled freebirdFormviewerViewItemsRadioControl\" jscontroller=\"EcW08c\" jsaction=\"click:cOuCgd; mousedown:UX7yZ; mouseup:lbsD7e; mouseleave:JywGue; touchstart:p6p2H; touchmove:FwuNnf; touchend:yfqBxc(preventMouseEvents=true|preventDefault=true); touchcancel:JMtRjd; focus:AHmuwe; blur:O22p3e; keydown:I481le; contextmenu:mg9Pef\" jsshadow=\"\" aria-label=\"Lindy Hop Workshop (270 CHF)\" tabindex=\"0\" data-value=\"Lindy Hop Workshop (270 CHF)\" aria-describedby=\"  i6\" role=\"radio\" aria-checked=\"false\" aria-posinset=\"1\" aria-setsize=\"3\">\r\n                                                    <div class=\"quantumWizTogglePaperradioInk exportInk\"></div>\r\n                                                    <div class=\"quantumWizTogglePaperradioInnerBox\"></div>\r\n                                                    <div class=\"quantumWizTogglePaperradioRadioContainer\">\r\n                                                        <div class=\"quantumWizTogglePaperradioOffRadio exportOuterCircle\">\r\n                                                            <div class=\"quantumWizTogglePaperradioOnRadio exportInnerCircle\"></div>\r\n                                                        </div>\r\n                                                    </div>\r\n                                                </div>\r\n                                                <div class=\"docssharedWizToggleLabeledContent\">\r\n                                                    <div class=\"docssharedWizToggleLabeledPrimaryText\">\r\n                                                        <span dir=\"auto\" class=\"docssharedWizToggleLabeledLabelText exportLabel freebirdFormviewerViewItemsRadioLabel\">Lindy Hop Workshop (270 CHF)</span>\r\n                                                    </div>\r\n                                                </div>\r\n                                            </div>\r\n                                        </label><label class=\"docssharedWizToggleLabeledContainer freebirdFormviewerViewItemsRadioChoice\">\r\n                                            <div class=\"exportLabelWrapper\">\r\n                                                <div class=\"quantumWizTogglePaperradioEl docssharedWizToggleLabeledControl freebirdThemedRadio freebirdThemedRadioDarkerDisabled freebirdFormviewerViewItemsRadioControl\" jscontroller=\"EcW08c\" jsaction=\"click:cOuCgd; mousedown:UX7yZ; mouseup:lbsD7e; mouseleave:JywGue; touchstart:p6p2H; touchmove:FwuNnf; touchend:yfqBxc(preventMouseEvents=true|preventDefault=true); touchcancel:JMtRjd; focus:AHmuwe; blur:O22p3e; keydown:I481le; contextmenu:mg9Pef\" jsshadow=\"\" aria-label=\"Solo Jazz Workshop (190 CHF)\" tabindex=\"-1\" data-value=\"Solo Jazz Workshop (190 CHF)\" aria-describedby=\"  i9\" role=\"radio\" aria-checked=\"false\" aria-posinset=\"2\" aria-setsize=\"3\">\r\n                                                    <div class=\"quantumWizTogglePaperradioInk exportInk\"></div>\r\n                                                    <div class=\"quantumWizTogglePaperradioInnerBox\"></div>\r\n                                                    <div class=\"quantumWizTogglePaperradioRadioContainer\">\r\n                                                        <div class=\"quantumWizTogglePaperradioOffRadio exportOuterCircle\">\r\n                                                            <div class=\"quantumWizTogglePaperradioOnRadio exportInnerCircle\"></div>\r\n                                                        </div>\r\n                                                    </div>\r\n                                                </div>\r\n                                                <div class=\"docssharedWizToggleLabeledContent\">\r\n                                                    <div class=\"docssharedWizToggleLabeledPrimaryText\">\r\n                                                        <span dir=\"auto\" class=\"docssharedWizToggleLabeledLabelText exportLabel freebirdFormviewerViewItemsRadioLabel\">Solo Jazz Workshop (190 CHF)</span>\r\n                                                    </div>\r\n                                                </div>\r\n                                            </div>\r\n                                        </label><label class=\"docssharedWizToggleLabeledContainer freebirdFormviewerViewItemsRadioChoice\">\r\n                                            <div class=\"exportLabelWrapper\">\r\n                                                <div class=\"quantumWizTogglePaperradioEl docssharedWizToggleLabeledControl freebirdThemedRadio freebirdThemedRadioDarkerDisabled freebirdFormviewerViewItemsRadioControl\" jscontroller=\"EcW08c\" jsaction=\"click:cOuCgd; mousedown:UX7yZ; mouseup:lbsD7e; mouseleave:JywGue; touchstart:p6p2H; touchmove:FwuNnf; touchend:yfqBxc(preventMouseEvents=true|preventDefault=true); touchcancel:JMtRjd; focus:AHmuwe; blur:O22p3e; keydown:I481le; contextmenu:mg9Pef\" jsshadow=\"\" aria-label=\"Ich will nur an die Parties!\" tabindex=\"0\" data-value=\"Ich will nur an die Parties!\" aria-describedby=\"  i12\" role=\"radio\" aria-checked=\"false\" aria-posinset=\"3\" aria-setsize=\"3\">\r\n                                                    <div class=\"quantumWizTogglePaperradioInk exportInk\"></div>\r\n                                                    <div class=\"quantumWizTogglePaperradioInnerBox\"></div>\r\n                                                    <div class=\"quantumWizTogglePaperradioRadioContainer\">\r\n                                                        <div class=\"quantumWizTogglePaperradioOffRadio exportOuterCircle\">\r\n                                                            <div class=\"quantumWizTogglePaperradioOnRadio exportInnerCircle\"></div>\r\n                                                        </div>\r\n                                                    </div>\r\n                                                </div>\r\n                                                <div class=\"docssharedWizToggleLabeledContent\">\r\n                                                    <div class=\"docssharedWizToggleLabeledPrimaryText\">\r\n                                                        <span dir=\"auto\" class=\"docssharedWizToggleLabeledLabelText exportLabel freebirdFormviewerViewItemsRadioLabel\">Ich will nur an die Parties!</span>\r\n                                                    </div>\r\n                                                </div>\r\n                                            </div>\r\n                                        </label>\r\n                                    </div>\r\n                                </content>\r\n                            </div><input type=\"hidden\" name=\"entry.89412826\" jsname=\"L9xHkb\" disabled=\"\">\r\n                        </div>\r\n                        <div class=\"freebirdFormviewerViewItemsItemGradingGradingBox freebirdFormviewerViewItemsItemGradingFeedbackBox\" jsname=\"R7fTud\"></div>\r\n                        <div jsname=\"XbIQze\" class=\"freebirdFormviewerViewItemsItemErrorMessage\" id=\"i.err.297145218\" role=\"alert\"></div>\r\n                    </div>\r\n                    <div role=\"listitem\" class=\"freebirdFormviewerViewItemsItemItem\" jsname=\"ibnC6b\" jscontroller=\"pkFYWb\" jsaction=\"sPvj8e:F0ZU4;\" data-required=\"true\" data-other-input=\"qSV85\" data-other-hidden=\"MfYA1e\" data-item-id=\"494424461\">\r\n                        <div class=\"freebirdFormviewerViewItemsItemItemHeader\">\r\n                            <div class=\"freebirdFormviewerViewItemsItemItemTitleContainer\">\r\n                                <div class=\"freebirdFormviewerViewItemsItemItemTitle\" dir=\"auto\" id=\"i15\" role=\"heading\" aria-level=\"2\" aria-describedby=\"i.desc.494424461\">\r\n                                    Bist du am Solo Friday dabei? <span class=\"freebirdFormviewerViewItemsItemRequiredAsterisk\" aria-hidden=\"true\">*</span>\r\n                                </div>\r\n                                <div class=\"freebirdFormviewerViewItemsItemItemHelpText\" id=\"i.desc.494424461\" dir=\"auto\"></div>\r\n                            </div>\r\n                        </div>\r\n                        <div jscontroller=\"eFy6Rc\" jsaction=\"sPvj8e:Gh295d\" jsname=\"cnAzRb\" data-input=\"L9xHkb\">\r\n                            <div jscontroller=\"wPRNsd\" jsshadow=\"\" jsaction=\"keydown: I481le;JIbuQc:JIbuQc;rcuQ6b:rcuQ6b\" jsname=\"wCJL8\" aria-labelledby=\"i15\" aria-describedby=\"i.desc.494424461 i.err.494424461\" aria-required=\"true\" role=\"radiogroup\">\r\n                                <content role=\"presentation\" jsname=\"bN97Pc\">\r\n                                    <div class=\"\">\r\n                                        <label class=\"docssharedWizToggleLabeledContainer freebirdFormviewerViewItemsRadioChoice\">\r\n                                            <div class=\"exportLabelWrapper\">\r\n                                                <div class=\"quantumWizTogglePaperradioEl docssharedWizToggleLabeledControl freebirdThemedRadio freebirdThemedRadioDarkerDisabled freebirdFormviewerViewItemsRadioControl\" jscontroller=\"EcW08c\" jsaction=\"click:cOuCgd; mousedown:UX7yZ; mouseup:lbsD7e; mouseleave:JywGue; touchstart:p6p2H; touchmove:FwuNnf; touchend:yfqBxc(preventMouseEvents=true|preventDefault=true); touchcancel:JMtRjd; focus:AHmuwe; blur:O22p3e; keydown:I481le; contextmenu:mg9Pef\" jsshadow=\"\" aria-label=\"Ja (30 CHF)\" tabindex=\"0\" data-value=\"Ja (30 CHF)\" aria-describedby=\"  i18\" role=\"radio\" aria-checked=\"false\" aria-posinset=\"1\" aria-setsize=\"2\">\r\n                                                    <div class=\"quantumWizTogglePaperradioInk exportInk\"></div>\r\n                                                    <div class=\"quantumWizTogglePaperradioInnerBox\"></div>\r\n                                                    <div class=\"quantumWizTogglePaperradioRadioContainer\">\r\n                                                        <div class=\"quantumWizTogglePaperradioOffRadio exportOuterCircle\">\r\n                                                            <div class=\"quantumWizTogglePaperradioOnRadio exportInnerCircle\"></div>\r\n                                                        </div>\r\n                                                    </div>\r\n                                                </div>\r\n                                                <div class=\"docssharedWizToggleLabeledContent\">\r\n                                                    <div class=\"docssharedWizToggleLabeledPrimaryText\">\r\n                                                        <span dir=\"auto\" class=\"docssharedWizToggleLabeledLabelText exportLabel freebirdFormviewerViewItemsRadioLabel\">Ja (30 CHF)</span>\r\n                                                    </div>\r\n                                                </div>\r\n                                            </div>\r\n                                        </label><label class=\"docssharedWizToggleLabeledContainer freebirdFormviewerViewItemsRadioChoice\">\r\n                                            <div class=\"exportLabelWrapper\">\r\n                                                <div class=\"quantumWizTogglePaperradioEl docssharedWizToggleLabeledControl freebirdThemedRadio freebirdThemedRadioDarkerDisabled freebirdFormviewerViewItemsRadioControl\" jscontroller=\"EcW08c\" jsaction=\"click:cOuCgd; mousedown:UX7yZ; mouseup:lbsD7e; mouseleave:JywGue; touchstart:p6p2H; touchmove:FwuNnf; touchend:yfqBxc(preventMouseEvents=true|preventDefault=true); touchcancel:JMtRjd; focus:AHmuwe; blur:O22p3e; keydown:I481le; contextmenu:mg9Pef\" jsshadow=\"\" aria-label=\"Nein\" tabindex=\"0\" data-value=\"Nein\" aria-describedby=\"  i21\" role=\"radio\" aria-checked=\"false\" aria-posinset=\"2\" aria-setsize=\"2\">\r\n                                                    <div class=\"quantumWizTogglePaperradioInk exportInk\"></div>\r\n                                                    <div class=\"quantumWizTogglePaperradioInnerBox\"></div>\r\n                                                    <div class=\"quantumWizTogglePaperradioRadioContainer\">\r\n                                                        <div class=\"quantumWizTogglePaperradioOffRadio exportOuterCircle\">\r\n                                                            <div class=\"quantumWizTogglePaperradioOnRadio exportInnerCircle\"></div>\r\n                                                        </div>\r\n                                                    </div>\r\n                                                </div>\r\n                                                <div class=\"docssharedWizToggleLabeledContent\">\r\n                                                    <div class=\"docssharedWizToggleLabeledPrimaryText\">\r\n                                                        <span dir=\"auto\" class=\"docssharedWizToggleLabeledLabelText exportLabel freebirdFormviewerViewItemsRadioLabel\">Nein</span>\r\n                                                    </div>\r\n                                                </div>\r\n                                            </div>\r\n                                        </label>\r\n                                    </div>\r\n                                </content>\r\n                            </div><input type=\"hidden\" name=\"entry.1342927410\" jsname=\"L9xHkb\" disabled=\"\">\r\n                        </div>\r\n                        <div class=\"freebirdFormviewerViewItemsItemGradingGradingBox freebirdFormviewerViewItemsItemGradingFeedbackBox\" jsname=\"R7fTud\"></div>\r\n                        <div jsname=\"XbIQze\" class=\"freebirdFormviewerViewItemsItemErrorMessage\" id=\"i.err.494424461\" role=\"alert\"></div>\r\n                    </div>\r\n                </div>\r\n                <div class=\"freebirdFormviewerViewNavigationNavControls\" jscontroller=\"lSvzH\" jsaction=\"rcuQ6b:npT2md;JIbuQc:Gl574d(QR6bsb),V3upec(GeGHKb),HiUbje(M2UYVd),NPBnCf(OCpkoe);\" data-shuffle-seed=\"-5556976566240279376\" data-should-execute-invisible-captcha-challenge=\"false\" data-is-receipt-checked=\"true\">\r\n                    <div class=\"freebirdFormviewerViewNavigationButtonsAndProgress\">\r\n                        <div class=\"freebirdFormviewerViewNavigationButtons\">\r\n                            <div role=\"button\" class=\"quantumWizButtonPaperbuttonEl quantumWizButtonPaperbuttonFlat quantumWizButtonPaperbutton2El2 freebirdFormviewerViewNavigationNoSubmitButton\" jscontroller=\"VXdfxd\" jsaction=\"click:cOuCgd; mousedown:UX7yZ; mouseup:lbsD7e; mouseenter:tfO1Yc; mouseleave:JywGue;touchstart:p6p2H; touchmove:FwuNnf; touchend:yfqBxc(preventMouseEvents=true|preventDefault=true); touchcancel:JMtRjd;focus:AHmuwe; blur:O22p3e; contextmenu:mg9Pef;\" jsshadow=\"\" jsname=\"OCpkoe\" aria-disabled=\"false\" tabindex=\"0\">\r\n                                <div class=\"quantumWizButtonPaperbuttonRipple exportInk\" jsname=\"ksKsZd\"></div>\r\n                                <div class=\"quantumWizButtonPaperbuttonFocusOverlay exportOverlay\"></div><content class=\"quantumWizButtonPaperbuttonContent\"><span class=\"quantumWizButtonPaperbuttonLabel exportLabel\">Weiter</span></content>\r\n                            </div>\r\n                        </div>\r\n                    </div>\r\n                    <div class=\"freebirdFormviewerViewNavigationPasswordWarning\">Geben Sie niemals Passw�rter �ber Google Formulare weiter.</div>\r\n                </div><input type=\"hidden\" name=\"fvv\" value=\"1\"><input type=\"hidden\" name=\"draftResponse\" value=\"[null,null,&quot;-5556976566240279376&quot;]\r\n\"><input type=\"hidden\" name=\"pageHistory\" value=\"0\"><input type=\"hidden\" name=\"fbzx\" value=\"-5556976566240279376\">\r\n            </div>\r\n        </div>\r\n    </form>\r\n</div>\r\n\r\n<!--<h1>Weather forecast</h1>\r\n\r\n<p>This component demonstrates fetching data from the server.</p>\r\n\r\n<p *ngIf=\"!forecasts\"><em>Loading...</em></p>\r\n\r\n<table class='table' *ngIf=\"forecasts\">\r\n    <thead>\r\n        <tr>\r\n            <th>Date</th>\r\n            <th>Temp. (C)</th>\r\n            <th>Temp. (F)</th>\r\n            <th>Summary</th>\r\n        </tr>\r\n    </thead>\r\n    <tbody>\r\n        <tr *ngFor=\"let forecast of forecasts\">\r\n            <td>{{ forecast.dateFormatted }}</td>\r\n            <td>{{ forecast.temperatureC }}</td>\r\n            <td>{{ forecast.temperatureF }}</td>\r\n            <td>{{ forecast.summary }}</td>\r\n        </tr>\r\n    </tbody>\r\n</table>-->";

/***/ }),
/* 22 */
/***/ (function(module, exports) {

module.exports = "<h1>Hello, world!</h1>\r\n<p>Welcome to your new single-page application, built with:</p>\r\n<ul>\r\n    <li><a href='https://get.asp.net/'>ASP.NET Core</a> and <a href='https://msdn.microsoft.com/en-us/library/67ef8sbd.aspx'>C#</a> for cross-platform server-side code</li>\r\n    <li><a href='https://angular.io/'>Angular</a> and <a href='http://www.typescriptlang.org/'>TypeScript</a> for client-side code</li>\r\n    <li><a href='https://webpack.github.io/'>Webpack</a> for building and bundling client-side resources</li>\r\n    <li><a href='http://getbootstrap.com/'>Bootstrap</a> for layout and styling</li>\r\n</ul>\r\n<p>To help you get started, we've also set up:</p>\r\n<ul>\r\n    <li><strong>Client-side navigation</strong>. For example, click <em>Counter</em> then <em>Back</em> to return here.</li>\r\n    <li><strong>Server-side prerendering</strong>. For faster initial loading and improved SEO, your Angular app is prerendered on the server. The resulting HTML is then transferred to the browser where a client-side copy of the app takes over.</li>\r\n    <li><strong>Webpack dev middleware</strong>. In development mode, there's no need to run the <code>webpack</code> build tool. Your client-side resources are dynamically built on demand. Updates are available as soon as you modify any file.</li>\r\n    <li><strong>Hot module replacement</strong>. In development mode, you don't even need to reload the page after making most changes. Within seconds of saving changes to files, your Angular app will be rebuilt and a new instance injected is into the page.</li>\r\n    <li><strong>Efficient production builds</strong>. In production mode, development-time features are disabled, and the <code>webpack</code> build tool produces minified static CSS and JavaScript files.</li>\r\n</ul>\r\n";

/***/ }),
/* 23 */
/***/ (function(module, exports) {

module.exports = "<div class='main-nav'>\r\n    <div class='navbar navbar-inverse'>\r\n        <div class='navbar-header'>\r\n            <button type='button' class='navbar-toggle' data-toggle='collapse' data-target='.navbar-collapse'>\r\n                <span class='sr-only'>Toggle navigation</span>\r\n                <span class='icon-bar'></span>\r\n                <span class='icon-bar'></span>\r\n                <span class='icon-bar'></span>\r\n            </button>\r\n            <a class='navbar-brand' [routerLink]=\"['/home']\">EventRegistratorWeb</a>\r\n        </div>\r\n        <div class='clearfix'></div>\r\n        <div class='navbar-collapse collapse'>\r\n            <ul class='nav navbar-nav'>\r\n                <li [routerLinkActive]=\"['link-active']\">\r\n                    <a [routerLink]=\"['/home']\">\r\n                        <span class='glyphicon glyphicon-home'></span> Home\r\n                    </a>\r\n                </li>\r\n                <li [routerLinkActive]=\"['link-active']\">\r\n                    <a [routerLink]=\"['/counter']\">\r\n                        <span class='glyphicon glyphicon-education'></span> Counter\r\n                    </a>\r\n                </li>\r\n                <li [routerLinkActive]=\"['link-active']\">\r\n                    <a [routerLink]=\"['/fetch-data']\">\r\n                        <span class='glyphicon glyphicon-th-list'></span> Fetch data\r\n                    </a>\r\n                </li>\r\n            </ul>\r\n        </div>\r\n    </div>\r\n</div>\r\n";

/***/ }),
/* 24 */
/***/ (function(module, exports, __webpack_require__) {


        var result = __webpack_require__(16);

        if (typeof result === "string") {
            module.exports = result;
        } else {
            module.exports = result.toString();
        }
    

/***/ }),
/* 25 */
/***/ (function(module, exports, __webpack_require__) {


        var result = __webpack_require__(17);

        if (typeof result === "string") {
            module.exports = result;
        } else {
            module.exports = result.toString();
        }
    

/***/ }),
/* 26 */
/***/ (function(module, exports, __webpack_require__) {


        var result = __webpack_require__(18);

        if (typeof result === "string") {
            module.exports = result;
        } else {
            module.exports = result.toString();
        }
    

/***/ }),
/* 27 */
/***/ (function(module, exports, __webpack_require__) {

module.exports = (__webpack_require__(0))(23);

/***/ }),
/* 28 */
/***/ (function(module, exports, __webpack_require__) {

module.exports = (__webpack_require__(0))(38);

/***/ }),
/* 29 */
/***/ (function(module, exports, __webpack_require__) {

module.exports = (__webpack_require__(0))(41);

/***/ }),
/* 30 */
/***/ (function(module, exports, __webpack_require__) {

module.exports = (__webpack_require__(0))(6);

/***/ }),
/* 31 */
/***/ (function(module, exports, __webpack_require__) {

module.exports = (__webpack_require__(0))(8);

/***/ }),
/* 32 */
/***/ (function(module, exports, __webpack_require__) {

module.exports = (__webpack_require__(0))(9);

/***/ })
/******/ ]);
//# sourceMappingURL=main-client.js.map