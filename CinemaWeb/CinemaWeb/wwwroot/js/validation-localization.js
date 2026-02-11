(function ($) {
    'use strict';

    if (!($ && $.validator)) {
        return;
    }

    const NUMBER_REGEX = /^-?(?:\d+|\d{1,3}(?:[\s\.,]\d{3})+)(?:[\.,]\d+)?$/;

    const parseNumber = (value) => {
        return parseFloat(value.toString().replace(',', '.'));
    };

    $.validator.methods.number = function (value, element) {
        return this.optional(element) || NUMBER_REGEX.test(value);
    };

    $.validator.methods.range = function (value, element, param) {
        if (this.optional(element)) {
            return true;
        }
        const val = parseNumber(value);
        return val >= param[0] && val <= param[1];
    };

    $.validator.methods.min = function (value, element, param) {
        return this.optional(element) || parseNumber(value) >= param;
    };

    $.validator.methods.max = function (value, element, param) {
        return this.optional(element) || parseNumber(value) <= param;
    };

})(jQuery);