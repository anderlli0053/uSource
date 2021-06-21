﻿using UnityEngine;
using System.Runtime.InteropServices;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using uSource.MathLib;

namespace uSource.Formats.Source.MDL
{
    [Flags]
    public enum StudioHDRFlags
    {
        // This flag is set if no hitbox information was specified
        STUDIOHDR_FLAGS_AUTOGENERATED_HITBOX = 0x00000001,

        // NOTE:  This flag is set at loadtime, not mdl build time so that we don't have to rebuild
        // models when we change materials.
        STUDIOHDR_FLAGS_USES_ENV_CUBEMAP = 0x00000002,

        // Use this when there are translucent parts to the model but we're not going to sort it 
        STUDIOHDR_FLAGS_FORCE_OPAQUE = 0x00000004,

        // Use this when we want to render the opaque parts during the opaque pass
        // and the translucent parts during the translucent pass
        STUDIOHDR_FLAGS_TRANSLUCENT_TWOPASS = 0x00000008,

        // This is set any time the .qc files has $staticprop in it
        // Means there's no bones and no transforms
        STUDIOHDR_FLAGS_STATIC_PROP = 0x00000010,

        // NOTE:  This flag is set at loadtime, not mdl build time so that we don't have to rebuild
        // models when we change materials.
        STUDIOHDR_FLAGS_USES_FB_TEXTURE = 0x00000020,

        // This flag is set by studiomdl.exe if a separate "$shadowlod" entry was present
        //  for the .mdl (the shadow lod is the last entry in the lod list if present)
        STUDIOHDR_FLAGS_HASSHADOWLOD = 0x00000040,

        // NOTE:  This flag is set at loadtime, not mdl build time so that we don't have to rebuild
        // models when we change materials.
        STUDIOHDR_FLAGS_USES_BUMPMAPPING = 0x00000080,

        // NOTE:  This flag is set when we should use the actual materials on the shadow LOD
        // instead of overriding them with the default one (necessary for translucent shadows)
        STUDIOHDR_FLAGS_USE_SHADOWLOD_MATERIALS = 0x00000100,

        // NOTE:  This flag is set when we should use the actual materials on the shadow LOD
        // instead of overriding them with the default one (necessary for translucent shadows)
        STUDIOHDR_FLAGS_OBSOLETE = 0x00000200,

        STUDIOHDR_FLAGS_UNUSED = 0x00000400,

        // NOTE:  This flag is set at mdl build time
        STUDIOHDR_FLAGS_NO_FORCED_FADE = 0x00000800,

        // NOTE:  The npc will lengthen the viseme check to always include two phonemes
        STUDIOHDR_FLAGS_FORCE_PHONEME_CROSSFADE = 0x00001000,

        // This flag is set when the .qc has $constantdirectionallight in it
        // If set, we use constantdirectionallightdot to calculate light intensity
        // rather than the normal directional dot product
        // only valid if STUDIOHDR_FLAGS_STATIC_PROP is also set
        STUDIOHDR_FLAGS_CONSTANT_DIRECTIONAL_LIGHT_DOT = 0x00002000,

        // Flag to mark delta flexes as already converted from disk format to memory format
        STUDIOHDR_FLAGS_FLEXES_CONVERTED = 0x00004000,

        // Indicates the studiomdl was built in preview mode
        STUDIOHDR_FLAGS_BUILT_IN_PREVIEW_MODE = 0x00008000,

        // Ambient boost (runtime flag)
        STUDIOHDR_FLAGS_AMBIENT_BOOST = 0x00010000,

        // Don't cast shadows from this model (useful on first-person models)
        STUDIOHDR_FLAGS_DO_NOT_CAST_SHADOWS = 0x00020000,

        // alpha textures should cast shadows in vrad on this model (ONLY prop_static!)
        STUDIOHDR_FLAGS_CAST_TEXTURE_SHADOWS = 0x00040000,


        // flagged on load to indicate no animation events on this model
        STUDIOHDR_FLAGS_VERT_ANIM_FIXED_POINT_SCALE = 0x00200000,
    }

    //TODO
    public class StudioStruct
    {
        public const byte VTXStripGroupTriListFlag = 0x01;
        public const byte VTXStripGroupTriStripFlag = 0x02;
        public const byte STUDIO_ANIM_RAWPOS = 0x01;
        public const byte STUDIO_ANIM_RAWROT = 0x02;
        public const byte STUDIO_ANIM_ANIMPOS = 0x04;
        public const byte STUDIO_ANIM_ANIMROT = 0x08;
        public const byte STUDIO_ANIM_DELTA = 0x10;
        public const int STUDIO_ANIM_RAWROT2 = 0x20;

        /// <summary>
        /// sizeof = 392
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public unsafe struct studiohdr_t
        {
            public Int32 id;
            public Int32 version;

            public Int32 checksum;

            //[MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 64)]
            //public Char[] name;

            private fixed byte _name[64];

            public string Name
            {
                get
                {
                    fixed (byte* name = _name)
                    {
                        return new string((sbyte*)name);
                    }
                }
            }

            public Int32 dataLength;

            public Vector3 eyeposition;
            public Vector3 illumposition;
            public Vector3 hull_min;
            public Vector3 hull_max;
            public Vector3 view_bbmin;
            public Vector3 view_bbmax;

            public StudioHDRFlags flags;

            // mstudiobone_t
            public Int32 bone_count;
            public Int32 bone_offset;

            // mstudiobonecontroller_t
            public Int32 bonecontroller_count;
            public Int32 bonecontroller_offset;

            // mstudiohitboxset_t
            public Int32 hitbox_count;
            public Int32 hitbox_offset;

            // mstudioanimdesc_t
            public Int32 localanim_count;
            public Int32 localanim_offset;

            // mstudioseqdesc_t
            public Int32 localseq_count;
            public Int32 localseq_offset;

            public Int32 activitylistversion;
            public Int32 eventsindexed;

            // mstudiotexture_t
            public Int32 texture_count;
            public Int32 texture_offset;

            public Int32 texturedir_count;
            public Int32 texturedir_offset;

            public Int32 skinreference_count;
            public Int32 skinrfamily_count;
            public Int32 skinreference_index;

            // mstudiobodyparts_t
            public Int32 bodypart_count;
            public Int32 bodypart_offset;

            // mstudioattachment_t
            public Int32 attachment_count;
            public Int32 attachment_offset;

            public Int32 localnode_count;
            public Int32 localnode_index;
            public Int32 localnode_name_index;

            // mstudioflexdesc_t
            public Int32 flexdesc_count;
            public Int32 flexdesc_index;

            // mstudioflexcontroller_t
            public Int32 flexcontroller_count;
            public Int32 flexcontroller_index;

            // mstudioflexrule_t
            public Int32 flexrules_count;
            public Int32 flexrules_index;

            // mstudioikchain_t
            public Int32 ikchain_count;
            public Int32 ikchain_index;

            // mstudiomouth_t
            public Int32 mouths_count;
            public Int32 mouths_index;

            // mstudioposeparamdesc_t
            public Int32 localposeparam_count;
            public Int32 localposeparam_index;

            public Int32 surfaceprop_index;

            public Int32 keyvalue_index;
            public Int32 keyvalue_count;

            // mstudioiklock_t
            public Int32 iklock_count;
            public Int32 iklock_index;

            public Single mass;
            public Int32 contents;

            // mstudiomodelgroup_t
            public Int32 includemodel_count;
            public Int32 includemodel_index;

            public Int32 virtualModel;
            // Placeholder for mutable-void*

            // mstudioanimblock_t
            public Int32 animblocks_name_index;
            public Int32 animblocks_count;
            public Int32 animblocks_index;

            public Int32 animblockModel;
            // Placeholder for mutable-void*

            public Int32 bonetablename_index;

            public Int32 vertex_base;
            public Int32 offset_base;

            // Used with $constantdirectionallight from the QC
            // Model should have flag #13 set if enabled
            public Byte directionaldotproduct;

            public Byte rootLod;
            // Preferred rather than clamped

            // 0 means any allowed, N means Lod 0 -> (N-1)
            public Byte numAllowedRootLods;

            public Byte unused;
            public Int32 unused2;

            // mstudioflexcontrollerui_t
            public Int32 flexcontrollerui_count;
            public Int32 flexcontrollerui_index;
        }

        /// <summary>
        /// sizeof = 216
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct mstudiobone_t
        {
            public Int32 sznameindex;
            public Int32 parent;

            [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 6)]
            public Int32[] bonecontroller;

            public Vector3 pos;
            public Quaternion quat;
            public Vector3 rot;

            public Vector3 posscale;
            public Vector3 rotscale;

            [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 12)]
            public Single[] poseToBone;

            public Quaternion qAlignment;
            public Int32 flags;
            public Int32 proctype;
            public Int32 procindex;
            public Int32 physicsbone;
            public Int32 surfacepropidx;
            public Int32 contents;

            [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 8)]
            public Int32[] unused;
        }

        //SEQUENCE

        [Serializable]
        public struct AniInfo
        {
            public string name;
            public mstudioanimdesc_t studioAnim;
            public List<AnimationBone> AnimationBones;
            public Keyframe[][] PosX;
            public Keyframe[][] PosY;
            public Keyframe[][] PosZ;

            public Keyframe[][] RotX;
            public Keyframe[][] RotY;
            public Keyframe[][] RotZ;
            public Keyframe[][] RotW;
        }

        [Serializable]
        public class AnimationBone
        {
            public byte Bone;
            public byte Flags;
            public int NumFrames;
            public Quaternion pQuat48;
            public Quaternion pQuat64;
            public Vector3 pVec48;
            public List<Vector3> FrameAngles;
            public List<Vector3> FramePositions;

            public AnimationBone(byte bone, byte flags, int numFrames)
            {
                Bone = bone;
                Flags = flags;
                NumFrames = numFrames;
                FramePositions = new List<Vector3>();
                FrameAngles = new List<Vector3>();
            }

            public void ReadData(uReader br)
            {
                var delta = (Flags & STUDIO_ANIM_DELTA) > 0;

                if ((Flags & STUDIO_ANIM_ANIMROT) > 0)
                {
                    // Why is this so painful :(
                    // Read the per-frame data using RLE, just like GoldSource models
                    var startPos = br.BaseStream.Position;
                    var offsets = br.ReadShortArray(3);
                    var endPos = br.BaseStream.Position;
                    var rotFrames = new List<float[]>();
                    for (var i = 0; i < NumFrames; i++) rotFrames.Add(new float[] { 0, 0, 0 });
                    for (var i = 0; i < 3; i++)
                    {
                        if (offsets[i] == 0) continue;
                        br.BaseStream.Position = startPos + offsets[i];
                        var values = br.ReadAnimationFrameValues(NumFrames);
                        for (var f = 0; f < values.Length; f++)
                        {
                            rotFrames[f][i] = +values[f];
                            if (f > 0 && delta) rotFrames[f][i] += values[f - 1];
                        }
                    }
                    FrameAngles.AddRange(rotFrames.Select(x => new Vector3(x[0], x[1], x[2])));
                    br.BaseStream.Position = endPos;
                }
                if ((Flags & STUDIO_ANIM_ANIMPOS) > 0)
                {
                    // Same as above, except for the position coordinate
                    var startPos = br.BaseStream.Position;
                    var offsets = br.ReadShortArray(3);
                    var endPos = br.BaseStream.Position;
                    var posFrames = new List<float[]>();
                    for (var i = 0; i < NumFrames; i++) posFrames.Add(new float[] { 0, 0, 0 });
                    for (var i = 0; i < 3; i++)
                    {
                        if (offsets[i] == 0) continue;
                        br.BaseStream.Position = startPos + offsets[i];
                        var values = br.ReadAnimationFrameValues(NumFrames);
                        for (var f = 0; f < values.Length; f++)
                        {
                            posFrames[f][i] = +values[f];
                            if (f > 0 && delta) posFrames[f][i] += values[f - 1];
                        }
                    }
                    FramePositions.AddRange(posFrames.Select(x => new Vector3(x[0], x[1], x[2])));
                    br.BaseStream.Position = endPos;
                }
                if ((Flags & STUDIO_ANIM_RAWROT) > 0)
                {
                    var quat48 = new Quaternion48();
                    br.ReadTypeFixed(ref quat48, 6);

                    this.pQuat48 = quat48.quaternion;
                }
                if ((Flags & STUDIO_ANIM_RAWROT2) > 0)
                {
                    var quat64 = new Quaternion64();
                    br.ReadTypeFixed(ref quat64, 8);

                    this.pQuat64 = quat64.quaternion;
                }
                if ((Flags & STUDIO_ANIM_RAWPOS) > 0)
                {
                    var vec48 = new Vector48();
                    br.ReadTypeFixed(ref vec48, 6);

                    this.pVec48 = vec48.ToVector3();
                }
            }
        }

        [Serializable]
        public struct SeqInfo
        {
            public string name;
            public mstudioseqdesc_t seq;
            public AniInfo ani;
        }

        /// <summary>
        /// sizeof = 100
        /// </summary>
        [Serializable]
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct mstudioanimdesc_t
        {
            public Int32 baseptr;
            public Int32 sznameindex;

            public Single fps;      // frames per second	
            public Int32 flags;     // looping/non-looping flags

            public Int32 numframes;

            // piecewise movement
            public Int32 nummovements;
            public Int32 movementindex;

            [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 6)]
            public Int32[] unused1;         // remove as appropriate (and zero if loading older versions)	

            public Int32 animblock;
            public Int32 animindex;  // non-zero when anim data isn't in sections

            public Int32 numikrules;
            public Int32 ikruleindex;   // non-zero when IK data is stored in the mdl
            public Int32 animblockikruleindex; // non-zero when IK data is stored in animblock file

            public Int32 numlocalhierarchy;
            public Int32 localhierarchyindex;

            public Int32 sectionindex;
            public Int32 sectionframes; // number of frames used in each fast lookup section, zero if not used

            public Int16 zeroframespan; // frames per span
            public Int16 zeroframecount; // number of spans
            public Int32 zeroframeindex;
            public Single zeroframestalltime;       // saved during read stalls
        };

        // sequence descriptions
        /// <summary>
        /// sizeof = 212
        /// </summary>
        [Serializable]
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct mstudioseqdesc_t
        {
            public Int32 baseptr;

            public Int32 szlabelindex;

            public Int32 szactivitynameindex;

            public Int32 flags;     // looping/non-looping flags

            public Int32 activity;  // initialized at loadtime to game DLL values
            public Int32 actweight;

            public Int32 numevents;
            public Int32 eventindex;

            public Vector3 bbmin;       // per sequence bounding box
            public Vector3 bbmax;

            public Int32 numblends;

            // Index into array of shorts which is groupsize[0] x groupsize[1] in length
            public Int32 animindexindex;

            public Int32 movementindex; // [blend] float array for blended movement
            [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 2)]
            public Int32[] groupsize;
            [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 2)]
            public Int32[] paramindex;  // X, Y, Z, XR, YR, ZR
            [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 2)]
            public Single[] paramstart; // local (0..1) starting value
            [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 2)]
            public Single[] paramend;   // local (0..1) ending value
            public Int32 paramparent;

            public Single fadeintime;       // ideal cross fate in time (0.2 default)
            public Single fadeouttime;  // ideal cross fade out time (0.2 default)

            public Int32 localentrynode;        // transition node at entry
            public Int32 localexitnode;     // transition node at exit
            public Int32 nodeflags;     // transition rules

            public Single entryphase;       // used to match entry gait
            public Single exitphase;        // used to match exit gait

            public Single lastframe;        // frame that should generation EndOfSequence

            public Int32 nextseq;       // auto advancing sequences
            public Int32 pose;          // index of delta animation between end and nextseq

            public Int32 numikrules;

            public Int32 numautolayers; //
            public Int32 autolayerindex;

            public Int32 weightlistindex;

            // FIXME: make this 2D instead of 2x1D arrays
            public Int32 posekeyindex;

            public Int32 numiklocks;
            public Int32 iklockindex;

            // Key values
            public Int32 keyvalueindex;
            public Int32 keyvaluesize;

            public Int32 cycleposeindex;        // index of pose parameter to use as cycle index

            public Int32 activitymodifierindex;
            public Int32 numactivitymodifiers;

            [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 5)]
            public Int32[] unused;      // remove/add as appropriate (grow back to 8 ints on version change!)
        };
        //SEQUENCE

        // intersection boxes
        /// <summary>
        /// sizeof = 68
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct mstudiobbox_t
        {
            public Int32 bone;
            public Int32 group;                 // intersection group
            public Vector3 bbmin;              // bounding box
            public Vector3 bbmax;
            public Int32 szhitboxnameindex;  // offset to the name of the hitbox.
            [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 8)]
            public Int32[] unused;
        };

        public struct Hitbox
        {
            public String Name;
            public mstudiobbox_t BBox;
        }

        /// <summary>
        /// sizeof = 12
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct mstudiohitboxset_t
        {
            public Int32 sznameindex;
            public Int32 numhitboxes;
            public Int32 hitboxindex;
        };

        /// <summary>
        /// sizeof = 64
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct mstudiotexture_t
        {
            public Int32 sznameindex;
            public Int32 flags;
            public Int32 used;
            public Int32 unused1;
            public Int32 material;
            public Int32 clientmaterial;

            [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 10)]
            public Int32[] unused;
        }

        public struct StudioBodyPart
        {
            public String Name;
            public StudioModel[] Models;
        }

        public struct StudioModel
        {
            public Boolean isBlank;
            public mstudiomodel_t Model;
            public Int32 NumLODs;
            public ModelLODHeader_t[] LODData;
            public mstudiomesh_t[] Meshes;
            public Dictionary<Int32, List<Int32>>[] IndicesPerLod;
            public mstudiovertex_t[][] VerticesPerLod;
        }

        /// <summary>
        /// sizeof = 16
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct mstudiobodyparts_t
        {
            public Int32 sznameindex;
            public Int32 nummodels;
            public Int32 _base;
            public Int32 modelindex;
        }

        /// <summary>
        /// sizeof = 148
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public unsafe struct mstudiomodel_t
        {
            //[MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 64)]
            //public Char[] name;
            private fixed byte _name[64];

            public string Name
            {
                get
                {
                    fixed (byte* name = _name)
                    {
                        return new string((sbyte*)name);
                    }
                }
            }

            public Int32 type;
            public Single boundingradius;
            public Int32 nummeshes;
            public Int32 meshindex;

            public Int32 numvertices;
            public Int32 vertexindex;
            public Int32 tangentsindex;

            public Int32 numattachments;
            public Int32 attachmentindex;

            public Int32 numeyeballs;
            public Int32 eyeballindex;

            public mstudio_modelvertexdata_t vertexdata;

            //[MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 8)]
            //public Int32[] unused;
            private fixed int _unused[8];

            public override string ToString()
            {
                return Name;
            }
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct mstudio_modelvertexdata_t
        {
            public Int32 vertexdata;
            public Int32 tangentdata;
        }

        // attachment
        public struct mstudioattachment_t
        {
            public Int32 sznameindex;
            public UInt16 flags;
            public Int32 localbone;
            //matrix3x4_t local; // attachment point
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public Vector3[] local;

            /*
             * localVec[0].x - localM11
             * localVec[0].y - localM12
             * localVec[0].z - localM13
             * localVec[1].x - localM14
             * localVec[1].y - localM21
             * localVec[1].z - localM22
             * localVec[2].x - localM23
             * localVec[2].y - localM24
             * localVec[2].z - localM31
             * localVec[3].x - localM32
             * localVec[3].y - localM33
             * localVec[3].z - localM34
             */

            /*
            // NOTE: Not sure this is correct row-column order.
            public float localM11;
            public float localM12;
            public float localM13;
            public float localM14;
            public float localM21;
            public float localM22;
            public float localM23;
            public float localM24;
            public float localM31;
            public float localM32;
            public float localM33;
            public float localM34;
            */

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            public Int32[] unused;
        }

        /// <summary>
        /// sizeof = 116
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public unsafe struct mstudiomesh_t
        {
            public Int32 material;
            public Int32 modelindex;
            public Int32 numvertices;
            public Int32 vertexoffset;
            public Int32 numflexes;
            public Int32 flexindex;
            public Int32 materialtype;
            public Int32 materialparam;
            public Int32 meshid;
            public Vector3 center;
            public mstudio_meshvertexdata_t VertexData;
            public fixed int _unused[8];
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct mstudio_meshvertexdata_t
        {
            public Int32 modelvertexdata;

            [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 8)]
            public Int32[] numlodvertices;
            //public int _modelVertexData;
            //public fixed int _numLodVertices[8];
        }

        /// <summary>
        /// sizeof = 64
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct vertexFileHeader_t
        {
            public Int32 id;
            public Int32 version;

            public Int32 checksum;

            public Int32 numLODs;

            [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 8)]
            public Int32[] numLODVertexes;

            public Int32 numFixups;

            public Int32 fixupTableStart;
            public Int32 vertexDataStart;
            public Int32 tangentDataStart;
        }

        /// <summary>
        /// sizeof = 12
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct vertexFileFixup_t
        {
            public Int32 lod;
            public Int32 sourceVertexID;
            public Int32 numVertexes;
        }

        /// <summary>
        /// sizeof = 48
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct mstudiovertex_t
        {
            public mstudioboneweight_t m_BoneWeights;
            public Vector3 m_vecPosition;
            public Vector3 m_vecNormal;
            public Vector2 m_vecTexCoord;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct mstudioboneweight_t
        {
            [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 3)]
            public Single[] weight;

            [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 3)]
            public Byte[] bone;

            public Byte numbones;
        }

        /// <summary>
        /// sizeof = 36
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct FileHeader_t
        {
            public Int32 version;

            public Int32 vertCacheSize;
            public UInt16 maxBonesPerStrip;
            public UInt16 maxBonesPerFace;
            public Int32 maxBonesPerVert;

            public Int32 checkSum;

            public Int32 numLODs;

            public Int32 materialReplacementListOffset;

            public Int32 numBodyParts;
            public Int32 bodyPartOffset;
        }

        /// <summary>
        /// sizeof = 8
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct BodyPartHeader_t
        {
            public Int32 numModels;
            public Int32 modelOffset;
        }

        /// <summary>
        /// sizeof = 8
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct ModelHeader_t
        {
            public Int32 numLODs;
            public Int32 lodOffset;
        }

        /// <summary>
        /// sizeof = 12
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct ModelLODHeader_t
        {
            public Int32 numMeshes;
            public Int32 meshOffset;
            public Single switchPoint;
        }

        /// <summary>
        /// sizeof = 9
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct MeshHeader_t
        {
            public Int32 numStripGroups;
            public Int32 stripGroupHeaderOffset;
            public Byte flags;
        }

        /// <summary>
        /// sizeof = 25 | 33
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct StripGroupHeader_t
        {
            public Int32 numVerts;
            public Int32 vertOffset;

            public Int32 numIndices;
            public Int32 indexOffset;

            public Int32 numStrips;
            public Int32 stripOffset;

            public Byte flags;

            //TODO: Some custom engines / games has this bytes, like a Alien Swarm / CSGO / DOTA2 (except L4D and L4D2?)
            //public Int32 numTopologyIndices;
            //public Int32 topologyOffset;
        }

        /// <summary>
        /// sizeof = 27 | 35
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct StripHeader_t
        {
            // indexOffset offsets into the mesh's index array.
            public Int32 numIndices;
            public Int32 indexOffset;

            // vertexOffset offsets into the mesh's vert array.
            public Int32 numVerts;
            public Int32 vertOffset;

            // use this to enable/disable skinning.  
            // May decide (in optimize.cpp) to put all with 1 bone in a different strip 
            // than those that need skinning.
            public Int16 numBones;

            public Byte flags;

            public Int32 numBoneStateChanges;
            public Int32 boneStateChangeOffset;

            //TODO: Some custom engines / games has this bytes, like a Alien Swarm / CSGO / DOTA2 (except L4D and L4D2?)
            // These go last on purpose!
            //public Int32 numTopologyIndices;
            //public Int32 topologyOffset;
        }

        /// <summary>
        /// sizeof = 9
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct Vertex_t
        {
            [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 3)]
            public Byte[] boneWeightIndices;

            public Byte numBones;

            public UInt16 origMeshVertId;

            [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 3)]
            public Byte[] boneID;
        }

        public class VTXMesh
        {
            public List<Vertex_t> Points { get; private set; }

            public VTXMesh()
            {
                Points = new List<Vertex_t>();
            }
        }

        public class VTXPoint
        {
            public byte[] BoneWeightIndices { get; private set; }
            public int NumBones { get; private set; }
            public short VertexIndex { get; private set; }
            public byte[] BoneIDs { get; private set; }

            public VTXPoint(byte[] boneWeightIndices, int numBones, short vertexIndex, byte[] boneIDs)
            {
                BoneWeightIndices = boneWeightIndices;
                NumBones = numBones;
                VertexIndex = vertexIndex;
                BoneIDs = boneIDs;
            }
        }
    }
}
