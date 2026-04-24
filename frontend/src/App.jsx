import { useState } from "react";

const API_BASE = import.meta.env.VITE_API_BASE ?? "http://localhost:5071";

async function apiRequest(path, options = {}, token) {
  const headers = new Headers(options.headers || {});

  if (!(options.body instanceof FormData) && !headers.has("Content-Type")) {
    headers.set("Content-Type", "application/json");
  }

  if (token) {
    headers.set("Authorization", `Bearer ${token}`);
  }

  const response = await fetch(`${API_BASE}${path}`, { ...options, headers });
  const contentType = response.headers.get("content-type") ?? "";
  const body = contentType.includes("application/json")
    ? await response.json()
    : await response.text();

  if (!response.ok) {
    const message =
      typeof body === "string"
        ? body
        : body?.message ?? JSON.stringify(body, null, 2);
    throw new Error(message || `Request failed with status ${response.status}`);
  }

  return body;
}

function getPostImageUrl(post) {
  return (
    post?.postUrl ||
    post?.imageUrl ||
    post?.url ||
    post?.publicUrl ||
    post?.fileUrl ||
    ""
  );
}

export default function App() {
  const [token, setToken] = useState("");
  const [myUserId, setMyUserId] = useState("");
  const [log, setLog] = useState("Pronto para testar.");
  const [loading, setLoading] = useState(false);

  const [name, setName] = useState("");
  const [userName, setUserName] = useState("");
  const [profilePictureUrl, setProfilePictureUrl] = useState("");
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");

  const [searchTerm, setSearchTerm] = useState("");
  const [timelineUserId, setTimelineUserId] = useState("");
  const [postType, setPostType] = useState("IMAGE");
  const [postFile, setPostFile] = useState(null);

  const [posts, setPosts] = useState([]);
  const [timeline, setTimeline] = useState([]);
  const [users, setUsers] = useState([]);

  async function run(action, successMessage = "OK") {
    setLoading(true);
    try {
      const result = await action();
      setLog(`${successMessage}\n\n${JSON.stringify(result, null, 2)}`);
      return result;
    } catch (error) {
      setLog(`ERRO: ${error.message}`);
      throw error;
    } finally {
      setLoading(false);
    }
  }

  async function createUser() {
    const created = await run(
      () =>
        apiRequest("/users", {
          method: "POST",
          body: JSON.stringify({
            name,
            userName,
            profilePictureUrl: profilePictureUrl || null,
            email,
            password
          })
        }),
      "Usuario criado"
    );

    if (created?.id) {
      setMyUserId(created.id);
      setTimelineUserId(created.id);
    }
  }

  async function login() {
    const result = await run(
      () =>
        apiRequest("/auth/login", {
          method: "POST",
          body: JSON.stringify({ email, password })
        }),
      "Login realizado"
    );

    const jwt = result?.accessToken ?? result?.token ?? "";
    setToken(jwt);
    setMyUserId(result?.userId ?? result?.id ?? myUserId);
  }

  async function loadFeed() {
    const result = await run(
      () => apiRequest("/posts", { method: "GET" }, token),
      "Feed carregado"
    );
    setPosts(Array.isArray(result) ? result : []);
  }

  async function publishPost() {
    if (!postFile) {
      setLog("Selecione um arquivo antes de publicar.");
      return;
    }

    const formData = new FormData();
    formData.append("file", postFile);
    formData.append("postType", postType);

    await run(
      () => apiRequest("/posts", { method: "POST", body: formData }, token),
      "Post publicado"
    );

    await loadFeed();
  }

  async function loadTimeline() {
    const target = timelineUserId || myUserId;
    if (!target) {
      setLog("Informe um UserId para timeline.");
      return;
    }

    const result = await run(
      () => apiRequest(`/timeline/${target}`),
      "Timeline carregada"
    );
    setTimeline(Array.isArray(result) ? result : []);
  }

  async function searchUsers() {
    const result = await run(
      () => apiRequest(`/search/users?q=${encodeURIComponent(searchTerm)}`),
      "Busca de usuarios concluida"
    );
    setUsers(Array.isArray(result) ? result : []);
  }

  return (
    <main className="page">
      <header className="hero">
        <h1>Mini Social App Tester</h1>
        <p>Use os passos abaixo para testar o fluxo completo via API Gateway.</p>
        <div className="pillRow">
          <span className="pill">Gateway: {API_BASE}</span>
          <span className="pill">
            Sessao: {token ? "Autenticado" : "Nao autenticado"}
          </span>
          <span className="pill">Meu UserId: {myUserId || "-"}</span>
        </div>
      </header>

      <section className="steps">
        <article className="card step">
          <h2>1. Criar conta</h2>
          <label>Nome</label>
          <input value={name} onChange={(event) => setName(event.target.value)} />
          <label>UserName</label>
          <input
            value={userName}
            onChange={(event) => setUserName(event.target.value)}
          />
          <label>Email</label>
          <input value={email} onChange={(event) => setEmail(event.target.value)} />
          <label>Senha</label>
          <input
            type="password"
            value={password}
            onChange={(event) => setPassword(event.target.value)}
          />
          <label>Foto de perfil (URL opcional)</label>
          <input
            value={profilePictureUrl}
            onChange={(event) => setProfilePictureUrl(event.target.value)}
          />
          <button disabled={loading} onClick={createUser}>
            Criar usuario
          </button>
        </article>

        <article className="card step">
          <h2>2. Login</h2>
          <p>Use o mesmo email e senha que voce cadastrou.</p>
          <label>Email</label>
          <input
            value={email}
            onChange={(event) => setEmail(event.target.value)}
            placeholder="seu@email.com"
          />
          <label>Senha</label>
          <input
            type="password"
            value={password}
            onChange={(event) => setPassword(event.target.value)}
            placeholder="********"
          />
          <button disabled={loading} onClick={login}>
            Entrar
          </button>
          <button
            disabled={loading || !token}
            onClick={() => run(() => apiRequest("/auth/me", { method: "GET" }, token), "Perfil carregado")}
          >
            Ver meu perfil
          </button>
        </article>

        <article className="card step">
          <h2>3. Publicar</h2>
          <label>Tipo do post</label>
          <input value={postType} onChange={(event) => setPostType(event.target.value)} />
          <label>Arquivo da imagem</label>
          <input
            type="file"
            onChange={(event) => setPostFile(event.target.files?.[0] ?? null)}
          />
          <button disabled={loading || !token} onClick={publishPost}>
            Publicar imagem
          </button>
        </article>

        <article className="card step">
          <h2>4. Explorar</h2>
          <button disabled={loading || !token} onClick={loadFeed}>
            Atualizar feed
          </button>
          <label>UserId para timeline</label>
          <input
            value={timelineUserId}
            onChange={(event) => setTimelineUserId(event.target.value)}
            placeholder={myUserId || "GUID"}
          />
          <button disabled={loading} onClick={loadTimeline}>
            Carregar timeline
          </button>
          <label>Buscar usuarios</label>
          <input
            value={searchTerm}
            onChange={(event) => setSearchTerm(event.target.value)}
            placeholder="nome, username ou email"
          />
          <button disabled={loading} onClick={searchUsers}>
            Buscar
          </button>
        </article>
      </section>

      <section className="contentGrid">
        <article className="card">
          <h2>Mini Feed</h2>
          <p className="hint">Publicacoes retornadas por `GET /posts`.</p>
          <div className="feedList">
            {posts.length === 0 && <p className="hint">Nenhum post carregado.</p>}
            {posts.map((post) => {
              const imageUrl = getPostImageUrl(post);
              return (
                <div key={post?.id ?? Math.random()} className="postCard">
                  {imageUrl ? (
                    <img src={imageUrl} alt="Post" className="postImage" />
                  ) : (
                    <div className="noImage">Sem preview de imagem</div>
                  )}
                  <div className="postMeta">
                    <strong>{post?.postType ?? "POST"}</strong>
                    <span>ID: {post?.id ?? "-"}</span>
                    <span>UserId: {post?.userId ?? "-"}</span>
                  </div>
                </div>
              );
            })}
          </div>
        </article>

        <article className="card">
          <h2>Timeline</h2>
          <p className="hint">Resultado de `GET /timeline/{`{userId}`}`.</p>
          <pre>{JSON.stringify(timeline, null, 2)}</pre>
        </article>

        <article className="card">
          <h2>Usuarios encontrados</h2>
          <p className="hint">Resultado de `GET /search/users?q=`.</p>
          <pre>{JSON.stringify(users, null, 2)}</pre>
        </article>

        <article className="card">
          <h2>Log tecnico</h2>
          <pre>{log}</pre>
        </article>
      </section>
    </main>
  );
}
