import { Server } from "@modelcontextprotocol/sdk/server/index.js";
import { StdioServerTransport } from "@modelcontextprotocol/sdk/server/stdio.js";
import {
  CallToolRequestSchema,
  ListToolsRequestSchema,
  Tool,
} from "@modelcontextprotocol/sdk/types.js";
import * as fs from "fs";
import * as path from "path";

// ============================================================
// draw.io XML ユーティリティ
// ============================================================

const EMPTY_GRAPH_XML =
  `<mxGraphModel dx="1422" dy="762" grid="1" gridSize="10" guides="1" ` +
  `tooltips="1" connect="1" arrows="1" fold="1" page="1" pageScale="1" ` +
  `pageWidth="1169" pageHeight="827" math="0" shadow="0">` +
  `<root><mxCell id="0" /><mxCell id="1" parent="0" /></root></mxGraphModel>`;

function generateId(): string {
  return (
    Math.random().toString(36).substring(2, 10) +
    Math.random().toString(36).substring(2, 10)
  );
}

function createEmptyFile(diagramName: string): string {
  return (
    `<mxfile host="Claude" modified="${new Date().toISOString()}" ` +
    `agent="drawio-mcp" version="21.0.0">` +
    `\n  <diagram id="${generateId()}" name="${diagramName}">` +
    `\n    ${EMPTY_GRAPH_XML}` +
    `\n  </diagram>\n</mxfile>`
  );
}

interface PageInfo {
  id: string;
  name: string;
  content: string;
}

function parsePages(xml: string): PageInfo[] {
  const pages: PageInfo[] = [];
  const re =
    /<diagram\s+[^>]*id="([^"]*)"[^>]*name="([^"]*)"[^>]*>([\s\S]*?)<\/diagram>/g;
  let m: RegExpExecArray | null;
  while ((m = re.exec(xml)) !== null) {
    pages.push({ id: m[1], name: m[2], content: m[3].trim() });
  }
  return pages;
}

function setPageContent(xml: string, pageId: string, content: string): string {
  return xml.replace(
    new RegExp(
      `(<diagram\\s+[^>]*id="${pageId}"[^>]*>)[\\s\\S]*?(<\\/diagram>)`,
    ),
    `$1\n    ${content}\n  $2`,
  );
}

function addPageToFile(
  xml: string,
  pageName: string,
  content?: string,
): string {
  const pageXml = content ?? EMPTY_GRAPH_XML;
  const newDiagram =
    `  <diagram id="${generateId()}" name="${pageName}">` +
    `\n    ${pageXml}\n  </diagram>`;
  return xml.replace(/<\/mxfile>/, `${newDiagram}\n</mxfile>`);
}

function removePageFromFile(xml: string, pageId: string): string {
  return xml.replace(
    new RegExp(`\\s*<diagram\\s+[^>]*id="${pageId}"[^>]*>[\\s\\S]*?<\\/diagram>`),
    "",
  );
}

function readFile(filePath: string): string {
  if (!fs.existsSync(filePath)) {
    throw new Error(`ファイルが見つかりません: ${filePath}`);
  }
  return fs.readFileSync(filePath, "utf-8");
}

function writeFile(filePath: string, content: string): void {
  fs.mkdirSync(path.dirname(filePath), { recursive: true });
  fs.writeFileSync(filePath, content, "utf-8");
}

function findDrawioFiles(dir: string, recursive: boolean): string[] {
  const results: string[] = [];
  for (const entry of fs.readdirSync(dir, { withFileTypes: true })) {
    const full = path.join(dir, entry.name);
    if (entry.isDirectory() && recursive) {
      results.push(...findDrawioFiles(full, true));
    } else if (entry.isFile() && entry.name.endsWith(".drawio")) {
      results.push(full);
    }
  }
  return results;
}

// ============================================================
// テンプレート定義
// ============================================================

const TEMPLATES: Record<string, string> = {
  // 空のmxGraphModel
  empty: `<mxGraphModel>
  <root>
    <mxCell id="0" />
    <mxCell id="1" parent="0" />
  </root>
</mxGraphModel>`,

  // 矩形
  rectangle: `<mxCell id="2" value="テキスト" style="rounded=1;whiteSpace=wrap;html=1;" vertex="1" parent="1">
  <mxGeometry x="160" y="160" width="120" height="60" as="geometry" />
</mxCell>`,

  // ひし形（条件分岐）
  diamond: `<mxCell id="2" value="条件" style="rhombus;whiteSpace=wrap;html=1;" vertex="1" parent="1">
  <mxGeometry x="160" y="160" width="120" height="80" as="geometry" />
</mxCell>`,

  // 円
  circle: `<mxCell id="2" value="テキスト" style="ellipse;whiteSpace=wrap;html=1;" vertex="1" parent="1">
  <mxGeometry x="160" y="160" width="100" height="100" as="geometry" />
</mxCell>`,

  // 接続線（矢印）
  arrow: `<mxCell id="3" value="" style="edgeStyle=orthogonalEdgeStyle;rounded=0;" edge="1" source="2" target="4" parent="1">
  <mxGeometry relative="1" as="geometry" />
</mxCell>`,

  // フローチャート（完全なmxGraphModel）
  flowchart: `<mxGraphModel dx="1422" dy="762" grid="1" gridSize="10" guides="1" tooltips="1" connect="1" arrows="1" fold="1" page="1" pageScale="1" pageWidth="1169" pageHeight="827" math="0" shadow="0">
  <root>
    <mxCell id="0" />
    <mxCell id="1" parent="0" />
    <mxCell id="2" value="開始" style="ellipse;whiteSpace=wrap;html=1;fillColor=#d5e8d4;strokeColor=#82b366;" vertex="1" parent="1">
      <mxGeometry x="480" y="80" width="120" height="60" as="geometry" />
    </mxCell>
    <mxCell id="3" value="処理1" style="rounded=1;whiteSpace=wrap;html=1;" vertex="1" parent="1">
      <mxGeometry x="480" y="200" width="120" height="60" as="geometry" />
    </mxCell>
    <mxCell id="4" value="条件" style="rhombus;whiteSpace=wrap;html=1;" vertex="1" parent="1">
      <mxGeometry x="470" y="320" width="140" height="80" as="geometry" />
    </mxCell>
    <mxCell id="5" value="終了" style="ellipse;whiteSpace=wrap;html=1;fillColor=#f8cecc;strokeColor=#b85450;" vertex="1" parent="1">
      <mxGeometry x="480" y="460" width="120" height="60" as="geometry" />
    </mxCell>
    <mxCell id="6" value="" style="edgeStyle=orthogonalEdgeStyle;" edge="1" source="2" target="3" parent="1">
      <mxGeometry relative="1" as="geometry" />
    </mxCell>
    <mxCell id="7" value="" style="edgeStyle=orthogonalEdgeStyle;" edge="1" source="3" target="4" parent="1">
      <mxGeometry relative="1" as="geometry" />
    </mxCell>
    <mxCell id="8" value="Yes" style="edgeStyle=orthogonalEdgeStyle;" edge="1" source="4" target="5" parent="1">
      <mxGeometry relative="1" as="geometry" />
    </mxCell>
  </root>
</mxGraphModel>`,

  // ERエンティティ（テーブル形式）
  er_entity: `<mxCell id="2" value="エンティティ名" style="shape=table;startSize=30;container=1;collapsible=1;childLayout=tableLayout;fixedRows=1;rowLines=0;fontStyle=1;align=center;resizeLast=1;" vertex="1" parent="1">
  <mxGeometry x="160" y="160" width="200" height="120" as="geometry" />
</mxCell>
<mxCell id="3" value="" style="shape=tableRow;horizontal=0;startSize=0;swimlaneHead=0;swimlaneBody=0;fillColor=none;collapsible=0;dropTarget=0;points=[[0,0.5],[1,0.5]];portConstraint=eastwest;fontSize=12;top=0;left=0;right=0;bottom=1;" vertex="1" parent="2">
  <mxGeometry y="30" width="200" height="30" as="geometry" />
</mxCell>
<mxCell id="4" value="PK" style="shape=partialRectangle;connectable=0;fillColor=none;top=0;left=0;bottom=0;right=0;fontStyle=1;overflow=hidden;" vertex="1" parent="3">
  <mxGeometry width="50" height="30" as="geometry"><mxRectangle width="50" height="30" as="alternateBounds" /></mxGeometry>
</mxCell>
<mxCell id="5" value="id" style="shape=partialRectangle;connectable=0;fillColor=none;top=0;left=0;bottom=0;right=0;overflow=hidden;" vertex="1" parent="3">
  <mxGeometry x="50" width="150" height="30" as="geometry"><mxRectangle width="150" height="30" as="alternateBounds" /></mxGeometry>
</mxCell>`,

  // スイムレーン
  swimlane: `<mxCell id="2" value="プロセス名" style="shape=pool;startSize=20;horizontal=1;" vertex="1" parent="1">
  <mxGeometry x="80" y="80" width="600" height="200" as="geometry" />
</mxCell>
<mxCell id="3" value="レーン1" style="swimlane;startSize=20;horizontal=0;" vertex="1" parent="2">
  <mxGeometry y="20" width="600" height="90" as="geometry" />
</mxCell>
<mxCell id="4" value="レーン2" style="swimlane;startSize=20;horizontal=0;" vertex="1" parent="2">
  <mxGeometry y="110" width="600" height="90" as="geometry" />
</mxCell>`,

  // シーケンス図（ライフライン + メッセージ）
  sequence: `<mxCell id="2" value="Actor A" style="shape=mxgraph.flowchart.start_2;fillColor=#dae8fc;strokeColor=#6c8ebf;" vertex="1" parent="1">
  <mxGeometry x="160" y="80" width="50" height="50" as="geometry" />
</mxCell>
<mxCell id="3" value="" style="endArrow=none;dashed=1;edgeStyle=elbowEdgeStyle;" edge="1" source="2" parent="1">
  <mxGeometry relative="1" as="geometry"><Array as="points"><mxPoint x="185" y="500" /></Array></mxGeometry>
</mxCell>
<mxCell id="4" value="Actor B" style="shape=mxgraph.flowchart.start_2;fillColor=#d5e8d4;strokeColor=#82b366;" vertex="1" parent="1">
  <mxGeometry x="400" y="80" width="50" height="50" as="geometry" />
</mxCell>
<mxCell id="5" value="" style="endArrow=none;dashed=1;edgeStyle=elbowEdgeStyle;" edge="1" source="4" parent="1">
  <mxGeometry relative="1" as="geometry"><Array as="points"><mxPoint x="425" y="500" /></Array></mxGeometry>
</mxCell>
<mxCell id="6" value="メッセージ1" style="edgeStyle=orthogonalEdgeStyle;endArrow=block;endFill=1;" edge="1" parent="1">
  <mxGeometry relative="1" as="geometry"><mxPoint x="185" y="200" as="sourcePoint" /><mxPoint x="425" y="200" as="targetPoint" /></mxGeometry>
</mxCell>`,
};

// ============================================================
// MCP サーバー定義
// ============================================================

const server = new Server(
  { name: "drawio-mcp", version: "1.0.0" },
  { capabilities: { tools: {} } },
);

const tools: Tool[] = [
  {
    name: "drawio_list",
    description:
      "指定ディレクトリ内の .drawio ファイルを一覧表示する。",
    inputSchema: {
      type: "object",
      properties: {
        directory: {
          type: "string",
          description: "検索するディレクトリパス（省略時はカレントディレクトリ）",
        },
        recursive: {
          type: "boolean",
          description: "サブディレクトリを再帰検索するか（デフォルト: false）",
        },
      },
    },
  },
  {
    name: "drawio_read",
    description:
      ".drawio ファイルを読み込んで XML 全体を返す。",
    inputSchema: {
      type: "object",
      required: ["file_path"],
      properties: {
        file_path: { type: "string", description: ".drawio ファイルのパス" },
      },
    },
  },
  {
    name: "drawio_write",
    description:
      "XML 全体を .drawio ファイルに書き込む（新規作成または上書き）。",
    inputSchema: {
      type: "object",
      required: ["file_path", "xml"],
      properties: {
        file_path: { type: "string", description: ".drawio ファイルのパス" },
        xml: {
          type: "string",
          description: "書き込む XML 全体（mxfile タグを含む）",
        },
      },
    },
  },
  {
    name: "drawio_create",
    description:
      "新しい空の .drawio ファイルを作成する（すでに存在する場合はエラー）。",
    inputSchema: {
      type: "object",
      required: ["file_path"],
      properties: {
        file_path: { type: "string", description: ".drawio ファイルのパス" },
        diagram_name: {
          type: "string",
          description: "最初のページ名（デフォルト: 'Diagram'）",
        },
      },
    },
  },
  {
    name: "drawio_list_pages",
    description:
      ".drawio ファイル内のページ（diagram）一覧を取得する。",
    inputSchema: {
      type: "object",
      required: ["file_path"],
      properties: {
        file_path: { type: "string", description: ".drawio ファイルのパス" },
      },
    },
  },
  {
    name: "drawio_get_page",
    description:
      ".drawio ファイル内の特定ページの mxGraphModel XML を取得する。",
    inputSchema: {
      type: "object",
      required: ["file_path", "page_id"],
      properties: {
        file_path: { type: "string", description: ".drawio ファイルのパス" },
        page_id: {
          type: "string",
          description: "ページ ID（drawio_list_pages で確認可能）",
        },
      },
    },
  },
  {
    name: "drawio_set_page",
    description:
      ".drawio ファイル内の特定ページの mxGraphModel XML を更新する。",
    inputSchema: {
      type: "object",
      required: ["file_path", "page_id", "content"],
      properties: {
        file_path: { type: "string", description: ".drawio ファイルのパス" },
        page_id: {
          type: "string",
          description: "更新するページの ID",
        },
        content: {
          type: "string",
          description: "新しい mxGraphModel XML",
        },
      },
    },
  },
  {
    name: "drawio_add_page",
    description: ".drawio ファイルに新しいページを追加する。",
    inputSchema: {
      type: "object",
      required: ["file_path", "page_name"],
      properties: {
        file_path: { type: "string", description: ".drawio ファイルのパス" },
        page_name: { type: "string", description: "新しいページの名前" },
        content: {
          type: "string",
          description:
            "初期コンテンツの mxGraphModel XML（省略時は空ページ）",
        },
      },
    },
  },
  {
    name: "drawio_delete_page",
    description: ".drawio ファイルからページを削除する。",
    inputSchema: {
      type: "object",
      required: ["file_path", "page_id"],
      properties: {
        file_path: { type: "string", description: ".drawio ファイルのパス" },
        page_id: {
          type: "string",
          description: "削除するページの ID",
        },
      },
    },
  },
  {
    name: "drawio_template",
    description:
      "よく使う図形・ダイアグラムの XML スニペットを取得する。" +
      "取得したスニペットを drawio_set_page / drawio_write に組み込んで使う。",
    inputSchema: {
      type: "object",
      required: ["template_type"],
      properties: {
        template_type: {
          type: "string",
          enum: [
            "empty",
            "rectangle",
            "diamond",
            "circle",
            "arrow",
            "flowchart",
            "er_entity",
            "swimlane",
            "sequence",
          ],
          description:
            "テンプレート種別: empty / rectangle / diamond / circle / arrow / " +
            "flowchart / er_entity / swimlane / sequence",
        },
      },
    },
  },
];

// ============================================================
// ハンドラー
// ============================================================

server.setRequestHandler(ListToolsRequestSchema, async () => ({ tools }));

server.setRequestHandler(CallToolRequestSchema, async (request) => {
  const { name, arguments: args } = request.params;

  try {
    switch (name) {
      // ---- drawio_list --------------------------------------------------
      case "drawio_list": {
        const dir = (args?.directory as string | undefined) ?? process.cwd();
        const recursive = (args?.recursive as boolean | undefined) ?? false;
        const files = findDrawioFiles(dir, recursive);
        return {
          content: [
            {
              type: "text",
              text:
                files.length > 0
                  ? `見つかったファイル (${files.length}件):\n${files.join("\n")}`
                  : ".drawio ファイルが見つかりませんでした",
            },
          ],
        };
      }

      // ---- drawio_read --------------------------------------------------
      case "drawio_read": {
        const xml = readFile(args!.file_path as string);
        return { content: [{ type: "text", text: xml }] };
      }

      // ---- drawio_write -------------------------------------------------
      case "drawio_write": {
        writeFile(args!.file_path as string, args!.xml as string);
        return {
          content: [
            { type: "text", text: `保存しました: ${args!.file_path}` },
          ],
        };
      }

      // ---- drawio_create ------------------------------------------------
      case "drawio_create": {
        const fp = args!.file_path as string;
        if (fs.existsSync(fp)) {
          return {
            content: [
              {
                type: "text",
                text: `エラー: ファイルがすでに存在します: ${fp}`,
              },
            ],
            isError: true,
          };
        }
        const diagramName = (args?.diagram_name as string | undefined) ?? "Diagram";
        writeFile(fp, createEmptyFile(diagramName));
        return {
          content: [{ type: "text", text: `作成しました: ${fp}` }],
        };
      }

      // ---- drawio_list_pages --------------------------------------------
      case "drawio_list_pages": {
        const xml = readFile(args!.file_path as string);
        const pages = parsePages(xml);
        const text =
          pages.length > 0
            ? `ページ一覧 (${pages.length}件):\n` +
              pages
                .map((p, i) => `  [${i + 1}] id="${p.id}"  name="${p.name}"`)
                .join("\n")
            : "ページが見つかりません";
        return { content: [{ type: "text", text }] };
      }

      // ---- drawio_get_page ----------------------------------------------
      case "drawio_get_page": {
        const xml = readFile(args!.file_path as string);
        const page = parsePages(xml).find((p) => p.id === args!.page_id);
        if (!page) {
          return {
            content: [
              {
                type: "text",
                text: `エラー: ページ ID "${args!.page_id}" が見つかりません`,
              },
            ],
            isError: true,
          };
        }
        return {
          content: [
            {
              type: "text",
              text: `ページ名: ${page.name}\n\n${page.content}`,
            },
          ],
        };
      }

      // ---- drawio_set_page ----------------------------------------------
      case "drawio_set_page": {
        const fp = args!.file_path as string;
        let xml = readFile(fp);
        xml = setPageContent(xml, args!.page_id as string, args!.content as string);
        writeFile(fp, xml);
        return {
          content: [
            {
              type: "text",
              text: `ページを更新しました (id="${args!.page_id}")`,
            },
          ],
        };
      }

      // ---- drawio_add_page ----------------------------------------------
      case "drawio_add_page": {
        const fp = args!.file_path as string;
        let xml = readFile(fp);
        xml = addPageToFile(
          xml,
          args!.page_name as string,
          args?.content as string | undefined,
        );
        writeFile(fp, xml);
        return {
          content: [
            {
              type: "text",
              text: `ページを追加しました: "${args!.page_name}"`,
            },
          ],
        };
      }

      // ---- drawio_delete_page -------------------------------------------
      case "drawio_delete_page": {
        const fp = args!.file_path as string;
        let xml = readFile(fp);
        xml = removePageFromFile(xml, args!.page_id as string);
        writeFile(fp, xml);
        return {
          content: [
            {
              type: "text",
              text: `ページを削除しました (id="${args!.page_id}")`,
            },
          ],
        };
      }

      // ---- drawio_template ----------------------------------------------
      case "drawio_template": {
        const tpl = TEMPLATES[args!.template_type as string];
        if (!tpl) {
          return {
            content: [
              {
                type: "text",
                text: `エラー: 不明なテンプレート: ${args!.template_type}`,
              },
            ],
            isError: true,
          };
        }
        return { content: [{ type: "text", text: tpl }] };
      }

      default:
        return {
          content: [{ type: "text", text: `エラー: 不明なツール: ${name}` }],
          isError: true,
        };
    }
  } catch (err) {
    const msg = err instanceof Error ? err.message : String(err);
    return {
      content: [{ type: "text", text: `エラー: ${msg}` }],
      isError: true,
    };
  }
});

// ============================================================
// 起動
// ============================================================

async function main() {
  const transport = new StdioServerTransport();
  await server.connect(transport);
  console.error("[drawio-mcp] サーバー起動完了");
}

main().catch((err) => {
  console.error("[drawio-mcp] 起動エラー:", err);
  process.exit(1);
});
