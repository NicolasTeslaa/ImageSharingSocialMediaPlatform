const rootPassword = process.env.MYSQL_ROOT_PASSWORD || "root123!";
const clusterName = process.env.MYSQL_CLUSTER_NAME || "postsCluster";
const clusterHost = process.env.MYSQL_CLUSTER_HOST || "127.0.0.1";
const clusterInstancesRaw = process.env.MYSQL_CLUSTER_INSTANCES;
const appUser = process.env.POSTS_APP_USER || "posts_app";
const appPassword = process.env.POSTS_APP_PASSWORD || "posts_app_123";
const postsDbName = process.env.POSTS_DB_NAME || "ImageSharingPostsDb";
const timelineDbName = process.env.TIMELINE_DB_NAME || "ImageSharingTimelineDb";
const usersDbName = process.env.USERS_DB_NAME || "ImageSharingUsersDb";

function addInstanceIfNeeded(cluster, uri) {
  try {
    cluster.addInstance(uri, { password: rootPassword, recoveryMethod: "clone" });
  } catch (error) {
    const message = String(error.message || error);
    if (message.includes("already part of this InnoDB Cluster")) {
      print(`${uri} already belongs to ${clusterName}.`);
      return;
    }

    throw error;
  }
}

function waitForInstance(uri) {
  for (let attempt = 1; attempt <= 60; attempt += 1) {
    try {
      shell.connect({ uri, password: rootPassword });
      session.runSql("SELECT 1");
      session.close();
      return;
    } catch (error) {
      print(`Waiting for ${uri} (${attempt}/60)...`);
      os.sleep(2);
    }
  }

  throw new Error(`Timed out while waiting for ${uri}.`);
}

const instances = clusterInstancesRaw
  ? clusterInstancesRaw.split(",").map((instance) => instance.trim()).filter(Boolean)
  : [
      `root@${clusterHost}:33061`,
      `root@${clusterHost}:33062`,
      `root@${clusterHost}:33063`
    ];

instances.forEach(waitForInstance);

shell.connect({ uri: instances[0], password: rootPassword });

for (let index = 0; index < instances.length; index += 1) {
  dba.configureInstance(instances[index], {
    clusterAdmin: "clusteradmin",
    clusterAdminPassword: rootPassword,
    password: rootPassword,
    interactive: false,
    restart: true
  });
}

shell.connect({ uri: instances[0], password: rootPassword });

let cluster;

try {
  cluster = dba.getCluster(clusterName);
  print(`Cluster ${clusterName} already exists.`);
} catch (error) {
  cluster = dba.createCluster(clusterName, {
    multiPrimary: false,
    force: true
  });
}

addInstanceIfNeeded(cluster, instances[1]);
addInstanceIfNeeded(cluster, instances[2]);

session.runSql(`CREATE DATABASE IF NOT EXISTS \`${postsDbName}\`;`);
session.runSql(`CREATE DATABASE IF NOT EXISTS \`${timelineDbName}\`;`);
session.runSql(`CREATE DATABASE IF NOT EXISTS \`${usersDbName}\`;`);
session.runSql(`CREATE USER IF NOT EXISTS '${appUser}'@'%' IDENTIFIED BY '${appPassword}';`);
session.runSql(`GRANT ALL PRIVILEGES ON \`${postsDbName}\`.* TO '${appUser}'@'%';`);
session.runSql(`GRANT ALL PRIVILEGES ON \`${timelineDbName}\`.* TO '${appUser}'@'%';`);
session.runSql(`GRANT ALL PRIVILEGES ON \`${usersDbName}\`.* TO '${appUser}'@'%';`);
session.runSql("FLUSH PRIVILEGES;");

print("Posts InnoDB Cluster configured successfully.");
