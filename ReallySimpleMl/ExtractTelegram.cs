using System.Collections.Generic;
using System.Threading.Tasks;
using TL;
using Console = Spectre.Console.AnsiConsole;

namespace ReallySimpleMl
{
    public static class ExtractTelegram
    {
        private static WTelegram.Client? Client;
        private static User? My;
        private static readonly Dictionary<long, User> Users = new();
        private static readonly Dictionary<long, ChatBase> Chats = new();

        public static async Task ExtrairDados()
        {
            Console.WriteLine("O programa exibirá as atualizações recebidas para o usuário logado. Pressione qualquer tecla para reiniciar o processo no caso de algum problema");

            while (true)
            {
                WTelegram.Helpers.Log = (_, s) => System.Diagnostics.Debug.WriteLine(s);
                Client = new WTelegram.Client(ConfigTelegram.LoginApi);
                using (Client)
                {
                    My = await Client.LoginUserIfNeeded();
                    Users[My.id] = My;

                    Console.WriteLine("Estamos logados como " +
                        $"{My.username ?? My.first_name + " " + My.last_name} (id {My.id})");

                    var dialogs = await Client.Messages_GetAllDialogs();
                    dialogs.CollectUsersChats(Users, Chats);

                    Client.OnUpdate += Client_OnUpdate;
                }
            }
        }

        private static async Task Client_OnUpdate(IObject arg)
        {
            if (arg is not UpdatesBase updates) return;
            updates.CollectUsersChats(Users, Chats);
            foreach (var update in updates.UpdateList)
            {
                switch (update)
                {
                    case UpdateNewMessage unm: await DisplayMessage(unm.message); break;
                    case UpdateEditMessage uem: await DisplayMessage(uem.message, true); break;
                    // Note: UpdateNewChannelMessage and UpdateEditChannelMessage are also handled by above cases
                    case UpdateDeleteChannelMessages udcm: Console.WriteLine($"{udcm.messages.Length} message(s) deleted in {Chat(udcm.channel_id)}"); break;
                    case UpdateDeleteMessages udm: Console.WriteLine($"{udm.messages.Length} message(s) deleted"); break;
                    //case UpdateUserTyping uut: Console.WriteLine($"{User(uut.user_id)} is {uut.action}"); break;
                    //case UpdateChatUserTyping ucut: Console.WriteLine($"{Peer(ucut.from_id)} is {ucut.action} in {Chat(ucut.chat_id)}"); break;
                    //case UpdateChannelUserTyping ucut2: Console.WriteLine($"{Peer(ucut2.from_id)} is {ucut2.action} in {Chat(ucut2.channel_id)}"); break;
                    //case UpdateChatParticipants { participants: ChatParticipants cp }: Console.WriteLine($"{cp.participants.Length} participants in {Chat(cp.chat_id)}"); break;
                    //case UpdateUserStatus uus: Console.WriteLine($"{User(uus.user_id)} is now {uus.status.GetType().Name[10..]}"); break;
                    //case UpdateUserName uun: Console.WriteLine($"{User(uun.user_id)} has changed profile name: {uun.first_name} {uun.last_name}"); break;
                    //case UpdateUser uu: Console.WriteLine($"{User(uu.user_id)} has changed infos/photo"); break;
                    default: Console.Write(""); break;
                }
            }
        }

        private static Task DisplayMessage(MessageBase messageBase, bool edit = false)
        {
            if (edit) Console.Write("(Edit): ");
            switch (messageBase)
            {
                case Message m: Console.WriteLine($"{Peer(m.from_id) ?? m.post_author} in {Peer(m.peer_id)}> {m.message}"); break;
                case MessageService ms: Console.WriteLine($"{Peer(ms.from_id)} in {Peer(ms.peer_id)} [{ms.action.GetType().Name[13..]}]"); break;
            }
            return Task.CompletedTask;
        }

        private static string User(long id) => Users.TryGetValue(id, out var user) ? user.ToString() : $"User {id}";

        private static string? Chat(long id) => Chats.TryGetValue(id, out var chat) ? chat.ToString() : $"Chat {id}";

        private static string? Peer(Peer peer) => peer is null ? null : peer is PeerUser user ? User(user.user_id)
            : peer is PeerChat or PeerChannel ? Chat(peer.ID) : $"Peer {peer.ID}";
    }
}
