﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using Syroot.BinaryData;
using System.IO;

namespace Lyzon.FileFormats.Layout
{
    public class BFLYT
    {
        public static Header header;

        public static LayoutSettings layoutSettings;
        public static TextureList textureList;
        public static FontList fontList;
        public static MaterialList materialList;

        public static Section currentSection;

        public class Header
        {
            public string signature;
            public int byteOrder, headerSize, version, fileSize, sectionCount;

            public Header(ref BinaryStream s)
            {
                signature = s.ReadString(4);
                byteOrder = (int)s.ReadUInt16();
                headerSize = (int)s.ReadUInt16();
                version = (int)s.ReadUInt32();
                fileSize = (int)s.ReadUInt32();
                sectionCount = (int)s.ReadUInt16();

                s.ReadUInt16(); // Reserved
            }
        }

        public class LayoutSettings : Section
        {
            public string name;
            public int originType;
            public float screenWidth, screenHeight, maxPartsWidth, maxPartsHeight;

            public LayoutSettings(ref BinaryStream s)
            {
                signature = currentSection.signature;
                size = currentSection.size;

                originType = s.Read1Byte();

                s.ReadBytes(3); // Padding

                screenWidth = s.ReadSingle();
                screenHeight = s.ReadSingle();
                maxPartsWidth = s.ReadSingle();
                maxPartsHeight = s.ReadSingle();

                name = s.ReadString(StringCoding.ZeroTerminated);

                s.Align(4); // Padding
            }

            public void Write(ref BinaryStream s)
            {
                s.WriteByte((byte)originType);

                s.Align(4); // Padding

                s.WriteSingle(screenWidth);
                s.WriteSingle(screenHeight);
                s.WriteSingle(maxPartsWidth);
                s.WriteSingle(maxPartsHeight);

                s.WriteString(name, StringCoding.ZeroTerminated);

                s.Align(4);
            }
        }

        public class Section
        {
            public string signature;
            public int size;
        }

        public Section ReadSection(ref BinaryStream s)
        {
            Section section = new Section();

            section.signature = s.ReadString(4);
            section.size = (int)s.ReadUInt32();

            currentSection = section;

            switch (section.signature)
            {
                case "lyt1":
                    layoutSettings = new LayoutSettings(ref s);

                    System.Windows.Forms.MessageBox.Show($"Read Layout Settings [{section.size}]");
                    break;
                case "txl1":
                    textureList = new TextureList(ref s);

                    System.Windows.Forms.MessageBox.Show($"Read Texture List [{section.size}]");
                    break;
                case "fnl1":
                    fontList = new FontList(ref s);

                    System.Windows.Forms.MessageBox.Show($"Read Font List [{section.size}]");
                    break;
                case "mat1":
                    materialList = new MaterialList(ref s);

                    System.Windows.Forms.MessageBox.Show($"Read Material List [{section.size}]");
                    break;
                case "usd1": // Temporary
                    s.ReadBytes(section.size - 8);
                    break;
            }

            return section;
        }

        public class TextureList : Section
        {
            public TextureList(ref BinaryStream s)
            {
                signature = currentSection.signature;
                size = currentSection.size;

                int startpos = (int)s.BaseStream.Position - 8;

                textureCount = s.ReadUInt16();

                s.Align(4); // Padding

                int basepos = (int)s.BaseStream.Position;

                fileNameOffsets = new uint[textureCount];
                fileNames = new string[textureCount];

                for (int o = 0; o < textureCount; o++)
                {
                    fileNameOffsets[o] = s.ReadUInt32();

                    int offsetpos = (int)s.BaseStream.Position;

                    s.BaseStream.Position = basepos + fileNameOffsets[o];

                    fileNames[o] = s.ReadString(StringCoding.ZeroTerminated);

                    s.Align(4);

                    s.BaseStream.Position = offsetpos;
                }

                s.BaseStream.Position = startpos + size;
            }

            public int textureCount;
            public uint[] fileNameOffsets;
            public string[] fileNames;
        }

        public class FontList : Section
        {
            public FontList(ref BinaryStream s)
            {
                signature = currentSection.signature;
                size = currentSection.size;

                int startpos = (int)s.BaseStream.Position - 8;

                fontCount = s.ReadUInt16();

                s.Align(4); // Padding

                int basepos = (int)s.BaseStream.Position;

                fileNameOffsets = new uint[fontCount];
                fileNames = new string[fontCount];

                for (int o = 0; o < fontCount; o++)
                {
                    fileNameOffsets[o] = s.ReadUInt32();

                    int offsetpos = (int)s.BaseStream.Position;

                    s.BaseStream.Position = basepos + fileNameOffsets[o];

                    fileNames[o] = s.ReadString(StringCoding.ZeroTerminated);

                    s.Align(4);

                    s.BaseStream.Position = offsetpos;
                }

                s.BaseStream.Position = startpos + size;
            }

            public int fontCount;
            public uint[] fileNameOffsets;
            public string[] fileNames;
        }

        public class MaterialList : Section
        {
            public MaterialList(ref BinaryStream s)
            {
                signature = currentSection.signature;
                size = currentSection.size;

                int startpos = (int)s.BaseStream.Position - 8;

                materialCount = s.ReadUInt16();

                s.Align(4); // Padding

                infoOffsets = new uint[materialCount];
                materials = new Material[materialCount];

                for (int o = 0; o < materialCount; o++)
                {
                    infoOffsets[o] = s.ReadUInt32();

                    int offsetpos = (int)s.BaseStream.Position;

                    s.BaseStream.Position = startpos + infoOffsets[o];

                    materials[o] = new Material(ref s);

                    s.BaseStream.Position = offsetpos;
                }

                s.BaseStream.Position = startpos + size;
            }

            public int materialCount;
            public uint[] infoOffsets;
            public Material[] materials;

            public class Material
            {
                public Material(ref BinaryStream s)
                {
                    name = Encoding.ASCII.GetString(s.ReadBytes(28)).Replace("\0", "");

                    flag = s.ReadUInt32();
                    unknown = s.ReadUInt32();

                    byte[] black = s.ReadBytes(4), white = s.ReadBytes(4);
                    blackColor = Color.FromArgb(black[3], black[0], black[1], black[2]);
                    whiteColor = Color.FromArgb(white[3], white[0], black[1], black[2]);

                    texMapCount = (uint)(flag & 0x03);
                    texSRTCount = (uint)((flag >> 2) & 0x03);
                    texCoordGenCount = (uint)((flag >> 4) & 0x03);
                    tevStageCount = (uint)((flag >> 6) & 0x07);
                    hasAlphaCompare = ((flag >> 9) & 0x01) == 1;
                    hasBlendMode = ((flag >> 10) & 0x01) == 1;
                    useTextureOnly = ((flag >> 11) & 0x01) == 1;
                    seperateBlendMode = ((flag >> 12) & 0x01) == 1;
                    hasIndirectParameter = ((flag >> 14) & 0x01) == 1;
                    projectionTexGenParameterCount = (uint)((flag >> 15) & 0x03);
                    hasFontShadowParameter = ((flag >> 17) & 0x01) == 1;
                    thresholingAlphaInterpolation = ((flag >> 18) & 0x01) == 1;

                    texMaps = new TexMap[texMapCount];
                    for (int i = 0; i < texMapCount; i++) texMaps[i] = new TexMap(ref s);

                    texSRTs = new TexSRT[texSRTCount];
                    for (int i = 0; i < texSRTCount; i++) texSRTs[i] = new TexSRT(ref s);

                    texCoords = new TexCoordGen[texCoordGenCount];
                    for (int i = 0; i < texCoordGenCount; i++) texCoords[i] = new TexCoordGen(ref s);

                    tevStages = new TevStage[tevStageCount];
                    for (int i = 0; i < tevStageCount; i++) tevStages[i] = new TevStage(ref s);

                    if (hasAlphaCompare) alphaCompare = new AlphaCompare(ref s);

                    if (hasBlendMode) blendMode = new BlendMode(ref s);

                    if (seperateBlendMode) blendAlpha = new BlendMode(ref s);

                    if (hasIndirectParameter) indirectParameter = new IndirectParameter(ref s);

                    projectionTexGenParameters = new ProjectionTexGenParameters[projectionTexGenParameterCount];
                    for (int i = 0; i < projectionTexGenParameterCount; i++) projectionTexGenParameters[i] = new ProjectionTexGenParameters(ref s);

                    if (hasFontShadowParameter) fontShadowParameter = new FontShadowParameter(ref s);

                    System.Windows.Forms.MessageBox.Show($"[{s.BaseStream.Position}] mat end");
                }

                uint texMapCount;
                uint texSRTCount;
                uint texCoordGenCount;
                uint tevStageCount;
                bool hasAlphaCompare;
                bool hasBlendMode;
                bool useTextureOnly;
                bool seperateBlendMode;
                bool hasIndirectParameter;
                uint projectionTexGenParameterCount;
                bool hasFontShadowParameter;
                bool thresholingAlphaInterpolation;

                TexMap[] texMaps;
                TexSRT[] texSRTs;
                TexCoordGen[] texCoords;
                TevStage[] tevStages;
                AlphaCompare alphaCompare;
                BlendMode blendMode;
                BlendMode blendAlpha;
                IndirectParameter indirectParameter;
                ProjectionTexGenParameters[] projectionTexGenParameters;
                FontShadowParameter fontShadowParameter;

                string name;
                uint flag, unknown;
                Color blackColor, whiteColor;

                class TexMap
                {
                    enum WrapMode
                    {
                        Clamp = 0,
                        Repeat = 1,
                        Mirror = 2
                    }

                    enum FilterMode
                    {
                        Near = 0,
                        Linear = 1
                    }

                    public TexMap(ref BinaryStream s)
                    {
                        textureIndex = s.ReadUInt16();

                        byte flag1 = s.Read1Byte();
                        byte flag2 = s.Read1Byte();

                        wrapS = (WrapMode)(flag1 & 0x3);
                        wrapT = (WrapMode)(flag2 & 0x3);

                        minFilter = (FilterMode)((flag1 >> 2) & 0x3);
                        maxFilter = (FilterMode)((flag2 >> 2) & 0x3);
                    }

                    // TODO | Write

                    int textureIndex;

                    WrapMode wrapS, wrapT;
                    FilterMode minFilter, maxFilter;
                }

                class TexSRT
                {
                    public TexSRT(ref BinaryStream s)
                    {
                        transX = s.ReadSingle();
                        transY = s.ReadSingle();
                        rotation = s.ReadSingle();
                        scaleX = s.ReadSingle();
                        scaleY = s.ReadSingle();
                    }

                    public void Write(ref BinaryStream s)
                    {
                        s.WriteSingle(transX);
                        s.WriteSingle(transY);
                        s.WriteSingle(rotation);
                        s.WriteSingle(scaleX);
                        s.WriteSingle(scaleY);
                    }

                    float transX, transY, rotation, scaleX, scaleY;
                }

                class TexCoordGen
                {
                    enum TexGenType
                    {
                        Matrix2x4 = 0
                    }

                    enum TexGenSource
                    {
                        Tex0 = 0,
                        Tex1 = 1,
                        Tex2 = 2,
                        Ortho = 3,
                        PaneBasedOrtho = 4,
                        Perspective = 5,
                        PaneBasedPerspective = 6,
                        BrickRepeat = 7
                    }

                    public TexCoordGen(ref BinaryStream s)
                    {
                        genType = (TexGenType)s.Read1Byte();
                        genSource = (TexGenSource)s.Read1Byte();

                        s.ReadBytes(2); // Padding

                        translate = s.ReadSingle();
                        scale = s.ReadSingle();
                        flag = s.Read1Byte();

                        s.ReadBytes(3); // Reserved
                    }

                    public void Write(ref BinaryStream s)
                    {
                        s.WriteByte((byte)genType);
                        s.WriteByte((byte)genSource);

                        s.Align(4); // Padding

                        s.WriteSingle(translate);
                        s.WriteSingle(scale);
                        s.WriteByte(flag);

                        s.Write(new byte[] { 0x00, 0x00, 0x00 }); // Reserved
                    }

                    TexGenType genType;
                    TexGenSource genSource;

                    float translate, scale;
                    byte flag;
                }

                class TevStage
                {
                    enum TevMode
                    {
                        Replace,
                        Modulate,
                        Add,
                        AddSigned,
                        Interpolate,
                        Subtract,
                        AddMultiply,
                        MultiplyAdd,
                        Overlay,
                        Lighten,
                        Darken,
                        Indirect,
                        BlendIndirect,
                        EachIndirect
                    }

                    public TevStage(ref BinaryStream s)
                    {
                        combineRBG = (TevMode)s.Read1Byte();
                        combineAlpha = (TevMode)s.Read1Byte();

                        s.ReadBytes(2); // Padding
                    }

                    public void Write(ref BinaryStream s)
                    {
                        s.WriteByte((byte)combineRBG);
                        s.WriteByte((byte)combineAlpha);

                        s.Align(4); // Padding
                    }

                    TevMode combineRBG, combineAlpha;
                }

                class AlphaCompare
                {
                    enum AlphaTest
                    {
                        Never,
                        Less,
                        LessEqual,
                        Equal,
                        NotEqual,
                        GreaterEqual,
                        Greater,
                        Always
                    }

                    public AlphaCompare(ref BinaryStream s)
                    {
                        compareFunc = (AlphaTest)s.ReadSingle();

                        s.ReadBytes(3); // Padding

                        alphaRef = s.ReadSingle();
                    }

                    public void Write(ref BinaryStream s)
                    {
                        s.WriteByte((byte)compareFunc);

                        s.Align(4); // Padding

                        s.WriteSingle(alphaRef);
                    }

                    AlphaTest compareFunc;
                    float alphaRef;
                }

                class BlendMode
                {
                    enum BlendOp
                    {
                        Disable,
                        Add,
                        Subtract,
                        ReverseSubtract,
                        SelectMin,
                        SelectMax
                    }

                    enum BlendFactor
                    {
                        Zero,
                        One,
                        DstColor,
                        InvDstColor,
                        SrcAlpha,
                        InvSrcAlpha,
                        DstAlpha,
                        InvDstAlpha,
                        SrcColor,
                        InvSrcColor
                    }

                    enum LogicOp
                    {
                        Disable,
                        NoOp,
                        Clear,
                        Set,
                        Copy,
                        InvCopy,
                        Inv,
                        And,
                        Nand,
                        Or,
                        Nor,
                        Xor,
                        Equiv,
                        RevAnd,
                        InvAnd,
                        RevOr,
                        InvOr
                    }

                    public BlendMode(ref BinaryStream s)
                    {
                        blendOperation = (BlendOp)s.Read1Byte();
                        srcBlendFactor = (BlendFactor)s.Read1Byte();
                        destBlendFactor = (BlendFactor)s.Read1Byte();
                        logicOperation = (LogicOp)s.Read1Byte();
                    }

                    public void Write(ref BinaryStream s)
                    {
                        s.WriteByte((byte)blendOperation);
                        s.WriteByte((byte)srcBlendFactor);
                        s.WriteByte((byte)destBlendFactor);
                        s.WriteByte((byte)logicOperation);
                    }

                    BlendOp blendOperation;
                    BlendFactor srcBlendFactor, destBlendFactor;
                    LogicOp logicOperation;
                }

                class IndirectParameter
                {
                    public IndirectParameter(ref BinaryStream s)
                    {
                        rotation = s.ReadSingle();
                        scaleX = s.ReadSingle();
                        scaleY = s.ReadSingle();
                    }

                    public void Write(ref BinaryStream s)
                    {
                        s.WriteSingle(rotation);
                        s.WriteSingle(scaleX);
                        s.WriteSingle(scaleY);
                    }

                    float rotation;
                    float scaleX, scaleY;
                }

                class ProjectionTexGenParameters
                {
                    public ProjectionTexGenParameters(ref BinaryStream s)
                    {
                        transX = s.ReadSingle();
                        transY = s.ReadSingle();
                        scaleX = s.ReadSingle();
                        scaleY = s.ReadSingle();

                        flag = s.Read1Byte();

                        s.Align(4); // Padding
                    }

                    public void Write(ref BinaryStream s)
                    {
                        s.WriteSingle(transX);
                        s.WriteSingle(transY);
                        s.WriteSingle(scaleX);
                        s.WriteSingle(scaleY);

                        s.WriteByte(flag);
                    }

                    float transX, transY, scaleX, scaleY;
                    byte flag;
                }

                class FontShadowParameter
                {
                    public FontShadowParameter(ref BinaryStream s)
                    {
                        byte[] black = s.ReadBytes(3), white = s.ReadBytes(4);

                        blackInterpolationColor = Color.FromArgb(255, black[0], black[1], black[2]);
                        whiteInterpolationColor = Color.FromArgb(white[3], white[0], white[1], white[2]);

                        s.Read1Byte(); // Reserved
                    }

                    public void Write(ref BinaryStream s)
                    {
                        s.WriteBytes(new byte[] { blackInterpolationColor.R, blackInterpolationColor.G, blackInterpolationColor.B });
                        s.WriteBytes(new byte[] { whiteInterpolationColor.R, whiteInterpolationColor.G, whiteInterpolationColor.B, whiteInterpolationColor.A });

                        s.WriteByte(0x00); // Reserved
                    }

                    Color blackInterpolationColor, whiteInterpolationColor;
                }
            }
        }

        public BFLYT(ref string filename)
        {
            BinaryStream s = new BinaryStream(new FileStream(filename, FileMode.Open));

            header = new Header(ref s);

            for (int i = 0; i < header.sectionCount; i++)
            {
                ReadSection(ref s);
            }

            System.Windows.Forms.MessageBox.Show($"Done");

            s.Flush();
            s.Dispose();
        }
    }
}