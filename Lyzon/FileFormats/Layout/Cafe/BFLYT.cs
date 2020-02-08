using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using Syroot.BinaryData;
using System.IO;
using Lyzon.Utils;

namespace Lyzon.FileFormats.Layout
{
    public class BFLYT
    {
        public static Header header;

        public static LayoutSettings layoutSettings;
        public static TextureList textureList;
        public static FontList fontList;
        public static MaterialList materialList;

        public static SectionHeader currentSection;

        public static Pane rootPane, parentPane, lastPane;

        public static Group rootGroup, parentGroup, lastGroup;

        public class Header
        {
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

            public void Write(ref BinaryStream s)
            {
                // Incomplete
            }

            public string signature;
            public int byteOrder, headerSize, version, fileSize, sectionCount;
        }

        public class LayoutSettings
        {
            public LayoutSettings(ref BinaryStream s)
            {
                SectionHeader section = new SectionHeader(ref s);

                s.Read1Byte();

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

            public SectionHeader sectionHeader;

            public string name;
            public int originType;
            public float screenWidth, screenHeight, maxPartsWidth, maxPartsHeight;
        }

        public void ReadSections(ref BinaryStream s)
        {
            SectionHeader section = new SectionHeader(ref s);

            currentSection = section;

            s.BaseStream.Position -= 8;

            switch (section.signature)
            {
                case "lyt1":
                    layoutSettings = new LayoutSettings(ref s);

                    System.Windows.Forms.MessageBox.Show($"Read Layout Settings [{section.signature}]");

                    layoutSettings.sectionHeader = section;
                    break;
                case "txl1":
                    textureList = new TextureList(ref s);

                    System.Windows.Forms.MessageBox.Show($"Read Texture List [{section.signature}]");

                    layoutSettings.sectionHeader = section;
                    break;
                case "fnl1":
                    fontList = new FontList(ref s);

                    System.Windows.Forms.MessageBox.Show($"Read Font List [{section.signature}]");

                    layoutSettings.sectionHeader = section;
                    break;
                case "mat1":
                    materialList = new MaterialList(ref s);

                    System.Windows.Forms.MessageBox.Show($"Read Material List [{section.signature}]");

                    layoutSettings.sectionHeader = section;
                    break;
                case "pan1":
                    Pane pane = new Pane(ref s);

                    if (rootPane == null)
                        rootPane = pane;

                    if (parentPane != null)
                    {
                        if (parentPane.children == null)
                            parentPane.children = new List<dynamic>();

                        parentPane.children.Add(pane);

                        pane.parent = parentPane;
                    }

                    lastPane = pane;

                    System.Windows.Forms.MessageBox.Show($"Read Pane [{section.signature}]");

                    lastPane.sectionHeader = section;
                    break;
                case "pic1":
                    Picture pic = new Picture(ref s);

                    if (parentPane != null)
                    {
                        if (parentPane.children == null)
                            parentPane.children = new List<dynamic>();

                        parentPane.children.Add(pic);

                        pic.parent = parentPane;
                    }

                    lastPane = pic;

                    System.Windows.Forms.MessageBox.Show($"Read Picture Pane [{section.signature}]");

                    lastPane.sectionHeader = section;
                    break;
                case "txt1":
                    TextBox txt = new TextBox(ref s);

                    if (parentPane != null)
                    {
                        if (parentPane.children == null)
                            parentPane.children = new List<dynamic>();

                        parentPane.children.Add(txt);

                        txt.parent = parentPane;
                    }

                    lastPane = txt;

                    System.Windows.Forms.MessageBox.Show($"Read TextBox Pane [{section.signature}]");

                    lastPane.sectionHeader = section;
                    break;
                case "wnd1":
                    Window wnd = new Window(ref s);

                    if (parentPane != null)
                    {
                        if (parentPane.children == null)
                            parentPane.children = new List<dynamic>();

                        parentPane.children.Add(wnd);

                        wnd.parent = parentPane;
                    }

                    lastPane = wnd;

                    System.Windows.Forms.MessageBox.Show($"Read Window Pane [{section.signature}]");

                    lastPane.sectionHeader = section;
                    break;
                case "bnd1":
                    Bounding bnd = new Bounding(ref s);

                    if (parentPane != null)
                    {
                        if (parentPane.children == null)
                            parentPane.children = new List<dynamic>();

                        parentPane.children.Add(bnd);

                        bnd.parent = parentPane;
                    }

                    lastPane = bnd;

                    System.Windows.Forms.MessageBox.Show($"Read Bounding Pane [{section.signature}]");

                    lastPane.sectionHeader = section;
                    break;
                case "cpt1":
                    Capture cpt = new Capture(ref s);

                    if (parentPane != null)
                    {
                        if (parentPane.children == null)
                            parentPane.children = new List<dynamic>();

                        parentPane.children.Add(cpt);

                        cpt.parent = parentPane;
                    }

                    lastPane = cpt;

                    System.Windows.Forms.MessageBox.Show($"Read Capture Pane [{section.signature}]");

                    lastPane.sectionHeader = section;
                    break;
                case "prt1":
                    s.ReadBytes(section.size);
                    break;
                case "ali1":
                    Alignment ali = new Alignment(ref s); // Read Alignment Pane later

                    if (parentPane != null)
                    {
                        if (parentPane.children == null)
                            parentPane.children = new List<dynamic>();

                        parentPane.children.Add(ali);

                        ali.parent = parentPane;
                    }

                    lastPane = ali;

                    System.Windows.Forms.MessageBox.Show($"Read Alignment Pane [{section.signature}]");

                    lastPane.sectionHeader = section;
                    break;
                case "scr1":
                    Scissor scr = new Scissor(ref s);

                    if (parentPane != null)
                    {
                        if (parentPane.children == null)
                            parentPane.children = new List<dynamic>();

                        parentPane.children.Add(scr);

                        scr.parent = parentPane;
                    }

                    lastPane = scr;

                    System.Windows.Forms.MessageBox.Show($"Read Scissor Pane [{section.signature}]");

                    lastPane.sectionHeader = section;
                    break;
                case "pas1":
                    if (lastPane != null)
                        parentPane = lastPane;

                    s.ReadBytes(8);
                    break;
                case "pae1":
                    lastPane = parentPane;
                    parentPane = lastPane != null ? lastPane.parent : null;

                    s.ReadBytes(8);
                    break;
                case "grp1":
                    Group group = new Group(ref s);

                    if (rootGroup == null)
                        rootGroup = group;

                    if (parentGroup != null)
                    {
                        if (parentGroup.children == null)
                            parentGroup.children = new List<Group>();

                        parentGroup.children.Add(group);

                        group.parent = parentGroup;
                    }

                    lastGroup = group;

                    System.Windows.Forms.MessageBox.Show($"Read Group [{section.signature}]");

                    lastGroup.sectionHeader = section;
                    break;
                case "grs1":
                    if (lastGroup != null)
                        parentGroup = lastGroup;

                    s.ReadBytes(8);
                    break;
                case "gre1":
                    lastGroup = parentGroup;
                    parentGroup = lastGroup != null ? lastGroup.parent : null;

                    s.ReadBytes(8);
                    break;
                case "usd1": // Temporary
                    s.ReadBytes(section.size);
                    break;
                case "cnt1": // Temporary
                    s.ReadBytes(section.size);
                    break;
            }
        }

        public class TextureList
        {
            public TextureList(ref BinaryStream s)
            {
                SectionHeader section = new SectionHeader(ref s);

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

                s.BaseStream.Position = startpos + section.size;
            }

            public void Write(ref BinaryStream s)
            {
                // Incomplete
            }

            public SectionHeader sectionHeader;

            public int textureCount;
            public uint[] fileNameOffsets;
            public string[] fileNames;
        }

        public class FontList
        {
            public FontList(ref BinaryStream s)
            {
                SectionHeader section = new SectionHeader(ref s);

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

                s.BaseStream.Position = startpos + section.size;
            }

            public void Write(ref BinaryStream s)
            {
                // Incomplete
            }

            public SectionHeader sectionHeader;

            public int fontCount;
            public uint[] fileNameOffsets;
            public string[] fileNames;
        }

        public class MaterialList
        {
            public MaterialList(ref BinaryStream s)
            {
                SectionHeader section = new SectionHeader(ref s);

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

                s.BaseStream.Position = startpos + section.size;
            }

            public void Write(ref BinaryStream s)
            {
                // Incomplete
            }

            public SectionHeader sectionHeader;

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

                    //System.Windows.Forms.MessageBox.Show($"[{s.BaseStream.Position}] mat end");
                }

                public void Write(ref BinaryStream s)
                {
                    // Incomplete
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

        public class Pane
        {
            public Pane(ref BinaryStream s)
            {
                ReadPane(this, ref s);
            }

            public static Pane ReadPane(Pane pane, ref BinaryStream s)
            {
                SectionHeader section = new SectionHeader(ref s);

                pane.flag = s.Read1Byte();
                pane.basePosition = s.Read1Byte();
                pane.alpha = s.Read1Byte();
                pane.extraFlag = s.Read1Byte();

                pane.name = Encoding.ASCII.GetString(s.ReadBytes(24)).Replace("\0", "");
                pane.userData = Encoding.ASCII.GetString(s.ReadBytes(8)).Replace("\0", "");

                pane.transX = s.ReadSingle();
                pane.transY = s.ReadSingle();
                pane.transZ = s.ReadSingle();

                pane.rotX = s.ReadSingle();
                pane.rotY = s.ReadSingle();
                pane.rotZ = s.ReadSingle();

                pane.scaleX = s.ReadSingle();
                pane.scaleY = s.ReadSingle();

                pane.sizeX = s.ReadSingle();
                pane.sizeY = s.ReadSingle();

                return pane;
            }

            public void Write(ref BinaryStream s)
            {
                s.WriteByte(flag);
                s.WriteByte(basePosition);
                s.WriteByte(alpha);
                s.WriteByte(extraFlag);

                s.WriteString(name, StringCoding.Raw);
                s.WriteString(userData, StringCoding.Raw);

                s.WriteSingle(transX);
                s.WriteSingle(transY);
                s.WriteSingle(transZ);

                s.WriteSingle(rotX);
                s.WriteSingle(rotY);
                s.WriteSingle(rotZ);

                s.WriteSingle(scaleX);
                s.WriteSingle(scaleY);

                s.WriteSingle(sizeX);
                s.WriteSingle(sizeY);
            }

            public dynamic parent;
            public List<dynamic> children;

            public SectionHeader sectionHeader;

            public string name, userData;
            public byte flag, basePosition, alpha, extraFlag;
            public float transX, transY, transZ, rotX, rotY, rotZ, scaleX, scaleY, sizeX, sizeY;
        }

        public class Picture : Pane
        {
            public Picture(ref BinaryStream s) : base(ref s)
            {
                vertexColors = new Color[4];

                vertexColors[0] = ColorHelper.BytesToColor(s.ReadBytes(4)); // TL
                vertexColors[1] = ColorHelper.BytesToColor(s.ReadBytes(4)); // TR
                vertexColors[2] = ColorHelper.BytesToColor(s.ReadBytes(4)); // BL
                vertexColors[3] = ColorHelper.BytesToColor(s.ReadBytes(4)); // BR

                materialIndex = s.ReadUInt16();
                texCoordCount = s.Read1Byte();
                flags = s.Read1Byte();

                texCoords = new TexCoord[texCoordCount];

                for (int i = 0; i < texCoordCount; i++)
                {
                    texCoords[i] = new TexCoord(
                        new Vec2(s.ReadSingle(), s.ReadSingle()), // TL
                        new Vec2(s.ReadSingle(), s.ReadSingle()), // TR
                        new Vec2(s.ReadSingle(), s.ReadSingle()), // BL
                        new Vec2(s.ReadSingle(), s.ReadSingle()) // BR
                    );
                }
            }

            public new void Write(ref BinaryStream s)
            {
                // Incomplete
            }

            public class TexCoord
            {
                public TexCoord(Vec2 topLeft, Vec2 topRight, Vec2 bottomLeft, Vec2 bottomRight)
                {
                    this.topLeft = topLeft;
                    this.topRight = topRight;
                    this.bottomLeft = bottomLeft;
                    this.bottomRight = bottomRight;
                }

                public Vec2 topLeft, topRight, bottomLeft, bottomRight;
            }

            Color[] vertexColors;

            int materialIndex, texCoordCount;
            byte flags;

            TexCoord[] texCoords;
        }

        public class TextBox : Pane
        {
            public TextBox(ref BinaryStream s) : base(ref s)
            {
                int startPos = (int)s.BaseStream.Position - 84;

                bufByteCount = s.ReadUInt16();
                stringByteCount = s.ReadUInt16();

                materialIndex = s.ReadUInt16();
                fontIndex = s.ReadUInt16();

                textPosition = s.Read1Byte();
                textAlignment = s.Read1Byte();

                flags = s.ReadUInt16();

                italicsRatio = s.ReadSingle();

                textOffset = s.ReadUInt32();

                textColors = new Color[2];

                textColors[0] = ColorHelper.BytesToColor(s.ReadBytes(4)); // Color 1
                textColors[1] = ColorHelper.BytesToColor(s.ReadBytes(4)); // Color 2

                fontSize = new Vec2(s.ReadSingle(), s.ReadSingle());

                charSpace = s.ReadSingle();
                lineSpace = s.ReadSingle();

                textIDOffset = s.ReadUInt32();

                shadowOffset = new Vec2(s.ReadSingle(), s.ReadSingle());
                shadowScale = new Vec2(s.ReadSingle(), s.ReadSingle());

                shadowColors = new Color[2];

                shadowColors[0] = ColorHelper.BytesToColor(s.ReadBytes(4)); // Color 1
                shadowColors[1] = ColorHelper.BytesToColor(s.ReadBytes(4)); // Color 2

                shadowItalicsRatio = s.ReadSingle();

                lineWidthOffsetOffset = s.ReadUInt32();

                perCharacterTransformOffset = s.ReadUInt32();

                text = Encoding.ASCII.GetString(s.ReadBytes(stringByteCount)).Replace("\0", "");

                if (textIDOffset != 0)
                {
                    s.BaseStream.Position = startPos + textIDOffset;
                    textID = (int)s.ReadUInt32();
                }

                s.Align(4); // Align everything

                // Do character transforms later
            }

            public new void Write(ref BinaryStream s)
            {
                // Incomplete
            }

            public int bufByteCount, stringByteCount, materialIndex, fontIndex;

            byte textPosition, textAlignment;

            public ushort flags;

            public float italicsRatio;

            public uint textOffset;

            public Color[] textColors;

            public Vec2 fontSize;

            public float charSpace, lineSpace;

            public uint textIDOffset;

            public Vec2 shadowOffset, shadowScale;

            public Color[] shadowColors;

            public float shadowItalicsRatio;

            public uint lineWidthOffsetOffset, perCharacterTransformOffset;

            public string text;

            public int textID;
        }

        public class Window : Pane
        {
            public Window(ref BinaryStream s) : base(ref s)
            {
                int startPos = (int)s.BaseStream.Position - 84;

                inflation = new WindowInflation();

                inflation.left = s.ReadUInt16();
                inflation.right = s.ReadUInt16();
                inflation.top = s.ReadUInt16();
                inflation.bottom = s.ReadUInt16();

                frameSize = new WindowFrameSize();

                frameSize.left = s.ReadUInt16();
                frameSize.right = s.ReadUInt16();
                frameSize.top = s.ReadUInt16();
                frameSize.bottom = s.ReadUInt16();

                frameCount = s.Read1Byte();
                windowFlags = s.Read1Byte();

                s.Align(4); // Padding

                contentOffset = s.ReadUInt32();
                frameOffsetTableOffset = s.ReadUInt32();

                s.BaseStream.Position = startPos + contentOffset;

                content = new WindowContent();

                content.vertexColors = new Color[4];

                content.vertexColors[0] = ColorHelper.BytesToColor(s.ReadBytes(4)); // TL
                content.vertexColors[1] = ColorHelper.BytesToColor(s.ReadBytes(4)); // TR
                content.vertexColors[2] = ColorHelper.BytesToColor(s.ReadBytes(4)); // BL
                content.vertexColors[3] = ColorHelper.BytesToColor(s.ReadBytes(4)); // BR

                content.materialIndex = s.ReadUInt16();
                content.texCoordCount = s.Read1Byte();

                s.Align(4); // Padding

                content.texCoords = new TexCoord[content.texCoordCount];

                for (int i = 0; i < content.texCoordCount; i++)
                {
                    content.texCoords[i] = new TexCoord(
                        new Vec2(s.ReadSingle(), s.ReadSingle()), // TL
                        new Vec2(s.ReadSingle(), s.ReadSingle()), // TR
                        new Vec2(s.ReadSingle(), s.ReadSingle()), // BL
                        new Vec2(s.ReadSingle(), s.ReadSingle()) // BR
                    );
                }

                s.BaseStream.Position = startPos + frameOffsetTableOffset;

                frameOffsetTable = new uint[frameCount];

                for (int i = 0; i < frameCount; i++)
                {
                    frameOffsetTable[i] = s.ReadUInt32();
                }

                frames = new WindowFrame[frameCount];

                for (int i = 0; i < frameCount; i++)
                {
                    s.BaseStream.Position = startPos + frameOffsetTable[i];

                    WindowFrame frame = new WindowFrame();

                    frame.materialIndex = s.ReadUInt16();
                    frame.textureFlip = s.Read1Byte();

                    s.Align(4); // Padding

                    frames[i] = frame;
                }
            }

            public new void Write(ref BinaryStream s)
            {
                // Incomplete
            }

            public class WindowInflation
            {
                public int left, right, top, bottom;
            }

            public class WindowFrameSize
            {
                public int left, right, top, bottom;
            }

            public class WindowContent
            {
                public Color[] vertexColors;

                public int materialIndex, texCoordCount;

                public TexCoord[] texCoords;
            }

            public class WindowFrame
            {
                public int materialIndex;
                public byte textureFlip;
            }

            public class TexCoord
            {
                public TexCoord(Vec2 topLeft, Vec2 topRight, Vec2 bottomLeft, Vec2 bottomRight)
                {
                    this.topLeft = topLeft;
                    this.topRight = topRight;
                    this.bottomLeft = bottomLeft;
                    this.bottomRight = bottomRight;
                }

                public Vec2 topLeft, topRight, bottomLeft, bottomRight;
            }

            public WindowInflation inflation;

            public WindowFrameSize frameSize;

            public int frameCount;
            public byte windowFlags;

            public uint contentOffset, frameOffsetTableOffset;

            public WindowContent content;

            public uint[] frameOffsetTable;

            public WindowFrame[] frames;
        }

        public class Bounding : Pane
        {
            public Bounding(ref BinaryStream s) : base(ref s)
            {
            }
        }
        public class Capture : Pane
        {
            public Capture(ref BinaryStream s) : base(ref s)
            {
            }
        }

        public class Parts : Pane
        {
            public Parts(ref BinaryStream s) : base(ref s)
            {
                vertexColors = new Color[4];

                vertexColors[0] = ColorHelper.BytesToColor(s.ReadBytes(4)); // TL
                vertexColors[1] = ColorHelper.BytesToColor(s.ReadBytes(4)); // TR
                vertexColors[2] = ColorHelper.BytesToColor(s.ReadBytes(4)); // BL
                vertexColors[3] = ColorHelper.BytesToColor(s.ReadBytes(4)); // BR

                materialIndex = s.ReadUInt16();
                texCoordCount = s.Read1Byte();
                flags = s.Read1Byte();

                texCoords = new TexCoord[texCoordCount];

                for (int i = 0; i < texCoordCount; i++)
                {
                    texCoords[i] = new TexCoord(
                        new Vec2(s.ReadSingle(), s.ReadSingle()), // TL
                        new Vec2(s.ReadSingle(), s.ReadSingle()), // TR
                        new Vec2(s.ReadSingle(), s.ReadSingle()), // BL
                        new Vec2(s.ReadSingle(), s.ReadSingle()) // BR
                    );
                }
            }

            public new void Write(ref BinaryStream s)
            {
                // Incomplete
            }

            public class TexCoord
            {
                public TexCoord(Vec2 topLeft, Vec2 topRight, Vec2 bottomLeft, Vec2 bottomRight)
                {
                    this.topLeft = topLeft;
                    this.topRight = topRight;
                    this.bottomLeft = bottomLeft;
                    this.bottomRight = bottomRight;
                }

                public Vec2 topLeft, topRight, bottomLeft, bottomRight;
            }

            Color[] vertexColors;

            int materialIndex, texCoordCount;
            byte flags;

            TexCoord[] texCoords;
        }

        public class Alignment : Pane // Later
        {
            public Alignment(ref BinaryStream s) : base(ref s)
            {
            }
        }

        public class Scissor : Pane
        {
            public Scissor(ref BinaryStream s) : base(ref s)
            {
            }
        }

        public class Group
        {
            public Group(ref BinaryStream s)
            {
                SectionHeader section = new SectionHeader(ref s);

                name = Encoding.ASCII.GetString(s.ReadBytes(33)).Replace("\0", "");

                s.Read1Byte(); // Padding

                paneCount = s.ReadUInt16();

                panes = new string[paneCount];

                for (int i = 0; i < paneCount; i++)
                {
                    panes[i] = Encoding.ASCII.GetString(s.ReadBytes(24)).Replace("\0", "");
                }
            }

            public void Write(ref BinaryStream s)
            {
                // Incomplete
            }

            public Group parent;
            public List<Group> children;

            public SectionHeader sectionHeader;

            public string name;

            public string[] panes;
            public int paneCount;
        }

        public class SectionHeader
        {
            public SectionHeader(ref BinaryStream s)
            {
                signature = s.ReadString(4);
                size = (int)s.ReadUInt32();
            }

            public void Write(ref BinaryStream s)
            {
                // Incomplete
            }

            public string signature;
            public int size;
        }

        public BFLYT(ref string filename)
        {
            BinaryStream s = new BinaryStream(new FileStream(filename, FileMode.Open));

            header = new Header(ref s);

            for (int i = 0; i < header.sectionCount; i++)
            {
                ReadSections(ref s);
            }

            System.Windows.Forms.MessageBox.Show($"[{s.BaseStream.Position}] stream end");

            System.Windows.Forms.MessageBox.Show($"Done");

            s.Flush();
            s.Dispose();
        }

        public void Write(ref BinaryStream s)
        {
            // Incomplete
        }
    }
}
