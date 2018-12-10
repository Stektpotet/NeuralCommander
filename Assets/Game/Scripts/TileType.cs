//  RIGHT   UP      LEFT    DOWN
//  1       1       1       1
[System.Flags]
enum TileType : byte //TODO use these as indices for tileset-lookup
{
    FLOOR = 0,
    WALL_SOUTH = 1,
    WALL_WEST = 2,
    CORNER_SOUTHWEST = 3,
    WALL_NORTH = 4,
    CORRIDOR_EASTWEST = 5,
    CORNER_NORTHWEST = 6,
    UNUSED_DOWN_LEFT_UP = 7,
    WALL_EAST = 8,
    CORNER_SOUTHEAST = 9,
    CORRIDOR_NORTHSOUTH = 10,
    UNUSED_DOWN_LEFT_RIGHT = 11,
    CORNER_NORTHEAST = 12,
    UNUSED_DOWN_UP_RIGHT = 13,
    UNUSED_UP_LEFT_RIGHT = 14,
    WALLED_IN = 15
}