local LuaUtil = CS.LuaUtil

local function CreateWindow(parentId, path, layer, property, args, callback)
    return LuaUtil.CreateWindow(parentId, path, layer, property, args, callback)
end

local function DestroyWindow(windowId, destroy)
    return LuaUtil.DestroyWindow(windowId, destroy)
end

local function IsNull(obj)
    return LuaUtil.IsNull(obj)
end

local function DontDestroyOnLoad(obj, isDontDestroy)
    LuaUtil.DontDestroyOnLoad(obj, isDontDestroy)
end

local function LoadScene(scene, callback)
    LuaUtil.LoadScene(scene, callback)
end

exports.CreateWindow = CreateWindow
exports.IsNull = IsNull
exports.DontDestroyOnLoad = DontDestroyOnLoad
exports.LoadScene = LoadScene
exports.DestroyWindow = DestroyWindow