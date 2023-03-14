namespace Common;

public class MessageCreator {
    public static ContainerMessage CreateChatMessage(ChatMessage message) {
        return new ContainerMessage() {
            Type = MessageType.ChatMessage,
            ChatMessage = message
        };
    }
    
    public static ContainerMessage CreateJoinMessage(JoinMessage message) {
        return new ContainerMessage() {
            Type = MessageType.JoinMessage,
            JoinMessage = message
        };
    }
}