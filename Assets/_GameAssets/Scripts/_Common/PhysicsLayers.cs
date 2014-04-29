public enum ePhysicsLayers 
{
	TERRAIN					= 1<<8,
	PLAYER  				= 1<<9,
	FLAG   					= 1<<10,
	DEBRIS  				= 1<<11,
	WALL					= 1<<12,
	CLIMB_AREA				= 1<<13,
	OBSTACLES				= 1<<14,
	FORBIDDEN_SPAWN_AREA	= 1<<15,
	UI						= 1<<31
}

public enum eIntPhysicsLayers 
{
	TERRAIN					= 8,
	PLAYER  				= 9,
	FLAG   					= 10,
	DEBRIS 	 				= 11,
	WALL					= 12,
	CLIMB_AREA				= 13,
	OBSTACLES				= 14,
	FORBIDDEN_SPAWN_AREA	= 15,
	UI						= 31
}