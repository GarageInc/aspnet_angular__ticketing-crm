function HashMap() {
    var map = {};

    this.clearMap = function() {
        map = {};
    }
    this.putValue = function(key, value) {
        var hash = hashCode(key);
        if (!map[hash]) {
            map[hash] = { k: key, v: value };
        }
    }

    this.removeValue = function(key) {
        var hash = hashCode(key);
        if (map[hash]) {
            delete map[hash];
        }
    }

    this.valueForKey = function(key) {
        var hash = hashCode(key);
        if (map[hash]) {
            return map[hash].v;
        } else {
            return undefined;
        }
    }

    this.hasKey = function(key) {
        var hash = hashCode(key);
        if (map[hash]) {
            return true;
        } else {
            return false;
        }
    }

    this.allKeys = function () {
        var res = [];
        Object.getOwnPropertyNames(map).forEach(function(val, idx, arr) {
           res.push(map[val].k);
        });
        return res;
    }

    //we assume no collisions -fck it it's not java
    function hashCode(string) {
        var hash = 0, i, chr, len;
        if (string.length === 0) return hash;
        for (i = 0, len = string.length; i < len; i++) {
            chr = string.charCodeAt(i);
            hash = ((hash << 5) - hash) + chr;
            hash |= 0; // Convert to 32bit integer
        }
        return ("0000000" + (hash >>> 0).toString(16)).substr(-8);;
    };

}
